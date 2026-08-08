using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SchoolManagement.BLL.DTOs.Import;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Helpers;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class StudentImportService : IStudentImportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ITenantSchemaProvisioner _schemaProvisioner;
    private readonly IStudentService _studentService;
    private readonly IStorageService _storageService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<StudentImportService> _logger;

    public StudentImportService(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ITenantSchemaProvisioner schemaProvisioner,
        IStudentService studentService,
        IStorageService storageService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<StudentImportService> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _schemaProvisioner = schemaProvisioner;
        _studentService = studentService;
        _storageService = storageService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public byte[] GetSampleCsv() => CsvImportHelper.BuildSampleCsv();

    public async Task<ImportResultDto> ProcessImportAsync(
        Guid classId,
        Guid sectionId,
        Stream csvStream,
        string fileName,
        long fileLength,
        CancellationToken cancellationToken = default)
    {
        EnsureTenant();
        EnsureCanManage();
        await EnsureReadyAsync(cancellationToken);

        CsvImportHelper.ValidateFile(fileName, fileLength);

        var clazz = await _unitOfWork.AdmissionLookups.GetClassByIdAsync(classId, cancellationToken)
            ?? throw new AppException("Invalid ClassId.", 400);
        var section = await _unitOfWork.AdmissionLookups.GetSectionByIdAsync(sectionId, cancellationToken)
            ?? throw new AppException("Invalid SectionId.", 400);
        if (section.ClassId != clazz.Id)
            throw new AppException("SectionId does not belong to ClassId.", 400);

        // Copy stream for parse + MinIO upload
        await using var buffer = new MemoryStream();
        await csvStream.CopyToAsync(buffer, cancellationToken);
        buffer.Position = 0;

        List<StudentImportRowDto> rows;
        try
        {
            rows = CsvImportHelper.ParseRows(buffer);
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new AppException($"Failed to parse CSV: {ex.Message}", 400);
        }

        var importedBy = GetCurrentUserId();
        var batch = new ImportBatch
        {
            Id = Guid.NewGuid(),
            ClassId = classId,
            SectionId = sectionId,
            FileName = Path.GetFileName(fileName),
            TotalRows = rows.Count,
            Status = ImportBatchStatuses.Processing,
            ImportedBy = importedBy,
            StartedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Imports.AddBatchAsync(batch, cancellationToken);
        await _unitOfWork.SaveTenantChangesAsync(cancellationToken);

        var slug = _tenantContext.TenantSlug
            ?? throw new AppException("Tenant slug is required for file storage.", 400);

        try
        {
            buffer.Position = 0;
            var objectKey = $"imports/{batch.Id}/{batch.FileName}";
            batch.FileUrl = await _storageService.UploadObjectAsync(
                slug, objectKey, buffer, "text/csv", cancellationToken);
            await _unitOfWork.Imports.UpdateBatchAsync(batch, cancellationToken);
            await _unitOfWork.SaveTenantChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to store import CSV in MinIO for batch {BatchId}", batch.Id);
        }

        var seenRegisterNos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var seenRolls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var failedRows = new List<ImportRowResultDto>();
        var successCount = 0;
        var failedCount = 0;

        foreach (var row in rows)
        {
            var validationError = await ValidateRowAsync(row, classId, sectionId, seenRegisterNos, seenRolls, cancellationToken);
            if (validationError is not null)
            {
                failedCount++;
                await RecordFailedRowAsync(batch.Id, row, validationError, cancellationToken);
                failedRows.Add(ToFailedResult(row, validationError));
                continue;
            }

            await _unitOfWork.BeginTenantTransactionAsync(cancellationToken);
            try
            {
                var student = await _studentService.CreateFromImportRowAsync(classId, sectionId, row, cancellationToken);
                await _unitOfWork.Imports.AddRowAsync(new ImportBatchRow
                {
                    Id = Guid.NewGuid(),
                    BatchId = batch.Id,
                    RowNumber = row.RowNumber,
                    RawData = JsonSerializer.Serialize(row.RawData),
                    Status = ImportBatchRowStatuses.Success,
                    StudentId = student.Id,
                    CreatedAt = DateTime.UtcNow
                }, cancellationToken);

                await _unitOfWork.SaveTenantChangesAsync(cancellationToken);
                await _unitOfWork.CommitTenantTransactionAsync(cancellationToken);
                successCount++;

                seenRegisterNos.Add(row.RegisterNo!);
                if (!string.IsNullOrWhiteSpace(row.Roll))
                    seenRolls.Add($"{row.AcademicYear}|{row.Roll}");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTenantTransactionAsync(cancellationToken);
                _unitOfWork.ClearTenantChangeTracker();

                var message = ex is AppException appEx ? appEx.Message : ex.Message;
                failedCount++;
                await RecordFailedRowAsync(batch.Id, row, message, cancellationToken);
                failedRows.Add(ToFailedResult(row, message));
                _logger.LogWarning(ex, "Import row {Row} failed in batch {BatchId}", row.RowNumber, batch.Id);
            }
        }

        batch.SuccessCount = successCount;
        batch.FailedCount = failedCount;
        batch.CompletedAt = DateTime.UtcNow;
        batch.Status = failedCount == 0
            ? ImportBatchStatuses.Completed
            : successCount == 0
                ? ImportBatchStatuses.Failed
                : ImportBatchStatuses.CompletedWithErrors;

        await _unitOfWork.Imports.UpdateBatchAsync(batch, cancellationToken);
        await _unitOfWork.SaveTenantChangesAsync(cancellationToken);

        return new ImportResultDto
        {
            BatchId = batch.Id,
            Status = batch.Status,
            TotalRows = batch.TotalRows,
            SuccessCount = successCount,
            FailedCount = failedCount,
            FailedRows = failedRows
        };
    }

    public async Task<ImportBatchListResponseDto> GetBatchesAsync(
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        EnsureTenant();
        EnsureCanManage();
        await EnsureReadyAsync(cancellationToken);

        var (items, total) = await _unitOfWork.Imports.GetBatchesAsync(page, pageSize, cancellationToken);
        return new ImportBatchListResponseDto
        {
            Items = items.Select(MapBatch).ToList(),
            Page = page < 1 ? 1 : page,
            PageSize = pageSize is < 1 or > 200 ? 20 : pageSize,
            TotalCount = total,
            TotalPages = pageSize <= 0 ? 0 : (int)Math.Ceiling(total / (double)Math.Max(pageSize, 1))
        };
    }

    public async Task<ImportBatchResponseDto> GetBatchAsync(Guid batchId, CancellationToken cancellationToken = default)
    {
        EnsureTenant();
        EnsureCanManage();
        await EnsureReadyAsync(cancellationToken);

        var batch = await _unitOfWork.Imports.GetBatchWithRowsAsync(batchId, cancellationToken)
            ?? throw new NotFoundException($"Import batch '{batchId}' not found.");

        var dto = MapBatch(batch);
        dto.Rows = batch.Rows.Select(r =>
        {
            var raw = DeserializeRaw(r.RawData);
            return new ImportRowResultDto
            {
                RowNumber = r.RowNumber,
                RegisterNo = GetRaw(raw, "RegisterNo"),
                FirstName = GetRaw(raw, "FirstName"),
                LastName = GetRaw(raw, "LastName"),
                ErrorMessage = r.ErrorMessage,
                RawData = raw
            };
        }).ToList();

        if (!string.IsNullOrWhiteSpace(batch.FileUrl) && !string.IsNullOrWhiteSpace(_tenantContext.TenantSlug))
        {
            try
            {
                dto.FileUrl = await _storageService.GetPresignedUrlAsync(
                    _tenantContext.TenantSlug, batch.FileUrl, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not generate presigned URL for import batch {Id}", batch.Id);
            }
        }

        return dto;
    }

    public async Task<(byte[] Content, string FileName)> GetFailedRowsCsvAsync(
        Guid batchId,
        CancellationToken cancellationToken = default)
    {
        EnsureTenant();
        EnsureCanManage();
        await EnsureReadyAsync(cancellationToken);

        _ = await _unitOfWork.Imports.GetBatchByIdAsync(batchId, cancellationToken)
            ?? throw new NotFoundException($"Import batch '{batchId}' not found.");

        var failed = await _unitOfWork.Imports.GetFailedRowsAsync(batchId, cancellationToken);
        var results = failed.Select(r =>
        {
            var raw = DeserializeRaw(r.RawData);
            return new ImportRowResultDto
            {
                RowNumber = r.RowNumber,
                RegisterNo = GetRaw(raw, "RegisterNo"),
                FirstName = GetRaw(raw, "FirstName"),
                LastName = GetRaw(raw, "LastName"),
                ErrorMessage = r.ErrorMessage,
                RawData = raw
            };
        }).ToList();

        return (CsvImportHelper.BuildFailedRowsCsv(results), $"failed_rows_{batchId}.csv");
    }

    private async Task<string?> ValidateRowAsync(
        StudentImportRowDto row,
        Guid classId,
        Guid sectionId,
        HashSet<string> seenRegisterNos,
        HashSet<string> seenRolls,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(row.RegisterNo))
            return "RegisterNo is required.";
        if (string.IsNullOrWhiteSpace(row.FirstName))
            return "FirstName is required.";
        if (string.IsNullOrWhiteSpace(row.MobileNo))
            return "MobileNo is required.";
        if (string.IsNullOrWhiteSpace(row.AcademicYear) || !int.TryParse(row.AcademicYear, out var year))
            return "AcademicYear is required and must be numeric.";
        if (string.IsNullOrWhiteSpace(row.AdmissionDate) ||
            !DateTime.TryParseExact(row.AdmissionDate, "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out _))
            return "AdmissionDate is required and must be Y-m-d (e.g. 2026-08-09).";

        if (!string.IsNullOrWhiteSpace(row.DateOfBirth) &&
            !DateTime.TryParseExact(row.DateOfBirth, "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out _))
            return "DateOfBirth must be Y-m-d if provided.";

        if (!string.IsNullOrWhiteSpace(row.Gender) &&
            !row.Gender.Equals("Male", StringComparison.OrdinalIgnoreCase) &&
            !row.Gender.Equals("Female", StringComparison.OrdinalIgnoreCase))
            return "Gender must be Male or Female if provided.";

        if (seenRegisterNos.Contains(row.RegisterNo))
            return "Duplicate RegisterNo within the same import file.";

        if (await _unitOfWork.Students.RegisterNoExistsAsync(row.RegisterNo.Trim(), null, cancellationToken))
            return "Register No already exists.";

        if (!string.IsNullOrWhiteSpace(row.Roll))
        {
            var rollKey = $"{year}|{row.Roll.Trim()}";
            if (seenRolls.Contains(rollKey))
                return "Duplicate Roll within the same import file for this academic year.";

            if (await _unitOfWork.Students.RollExistsAsync(
                    row.Roll.Trim(), classId, sectionId, year, null, cancellationToken))
                return "Roll already exists for this class, section, and academic year.";
        }

        if (!string.IsNullOrWhiteSpace(row.CategoryId))
        {
            if (!Guid.TryParse(row.CategoryId, out var catId))
                return "CategoryId must be a valid GUID.";
            if (await _unitOfWork.AdmissionLookups.GetCategoryByIdAsync(catId, cancellationToken) is null)
                return "CategoryId does not exist.";
        }

        return null;
    }

    private async Task RecordFailedRowAsync(
        Guid batchId,
        StudentImportRowDto row,
        string error,
        CancellationToken cancellationToken)
    {
        await _unitOfWork.Imports.AddRowAsync(new ImportBatchRow
        {
            Id = Guid.NewGuid(),
            BatchId = batchId,
            RowNumber = row.RowNumber,
            RawData = JsonSerializer.Serialize(row.RawData),
            Status = ImportBatchRowStatuses.Failed,
            ErrorMessage = error,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);
        await _unitOfWork.SaveTenantChangesAsync(cancellationToken);
    }

    private static ImportRowResultDto ToFailedResult(StudentImportRowDto row, string error) => new()
    {
        RowNumber = row.RowNumber,
        RegisterNo = row.RegisterNo,
        FirstName = row.FirstName,
        LastName = row.LastName,
        ErrorMessage = error,
        RawData = row.RawData
    };

    private static ImportBatchResponseDto MapBatch(ImportBatch b) => new()
    {
        Id = b.Id,
        FileName = b.FileName,
        FileUrl = b.FileUrl,
        ClassName = b.Class?.Name,
        SectionName = b.Section?.Name,
        TotalRows = b.TotalRows,
        SuccessCount = b.SuccessCount,
        FailedCount = b.FailedCount,
        Status = b.Status,
        ImportedBy = b.ImportedBy,
        StartedAt = b.StartedAt,
        CompletedAt = b.CompletedAt
    };

    private static Dictionary<string, string> DeserializeRaw(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                   ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static string? GetRaw(Dictionary<string, string> raw, string key)
        => raw.TryGetValue(key, out var v) ? v : null;

    private async Task EnsureReadyAsync(CancellationToken cancellationToken)
    {
        await _schemaProvisioner.EnsureAdmissionModuleAsync(_tenantContext.SchemaName!, cancellationToken);
        await _schemaProvisioner.EnsureStudentImportModuleAsync(_tenantContext.SchemaName!, cancellationToken);
    }

    private void EnsureTenant()
    {
        if (string.IsNullOrEmpty(_tenantContext.SchemaName))
            throw new AppException("X-Tenant-ID header is required.", 400);
    }

    private void EnsureCanManage()
    {
        var roles = GetCurrentRoles();
        if (roles.Contains(AppConstants.Roles.SuperAdmin) || roles.Contains(AppConstants.Roles.Admin))
            return;
        throw new ForbiddenException("Only Super Admin or School Admin can import students.");
    }

    private Guid GetCurrentUserId()
    {
        var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(AppConstants.Claims.UserId)
            ?? _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)
            ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub");

        if (claim is null || !Guid.TryParse(claim.Value, out var id))
            throw new UnauthorizedException();
        return id;
    }

    private HashSet<string> GetCurrentRoles()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user is null)
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var roles = user.FindAll("role")
            .Concat(user.FindAll(ClaimTypes.Role))
            .Select(c => c.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var rolesCsv = user.FindFirst(AppConstants.Claims.Roles)?.Value;
        if (!string.IsNullOrWhiteSpace(rolesCsv))
        {
            foreach (var r in rolesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                roles.Add(r);
        }

        return roles;
    }
}
