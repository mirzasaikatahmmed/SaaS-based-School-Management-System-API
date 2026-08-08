using System.Globalization;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.StudentDetails;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class LoginDeactivateService : ILoginDeactivateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ITenantSchemaProvisioner _schemaProvisioner;
    private readonly IStorageService _storageService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LoginDeactivateService(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ITenantSchemaProvisioner schemaProvisioner,
        IStorageService storageService,
        IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _schemaProvisioner = schemaProvisioner;
        _storageService = storageService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<LoginDeactivateListResponseDto> GetListAsync(
        LoginDeactivateFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        EnsureTenant();
        EnsureCanManage();
        await EnsureReadyAsync(cancellationToken);

        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize is < 1 or > 200 ? 20 : filter.PageSize;

        var (items, total) = await _unitOfWork.Students.SearchAsync(new StudentSearchFilter
        {
            ClassId = filter.ClassId,
            SectionId = filter.SectionId,
            Search = filter.Search,
            IsActive = null,
            IsLoginActive = false,
            SortBy = "name",
            SortDir = "asc",
            Page = page,
            PageSize = pageSize
        }, cancellationToken);

        var data = new List<LoginDeactivateListItemDto>(items.Count);
        foreach (var s in items)
            data.Add(await MapItemAsync(s, cancellationToken));

        return new LoginDeactivateListResponseDto
        {
            Data = data,
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize)
        };
    }

    public async Task ActivateAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        await SetLoginActiveAsync(studentId, true, cancellationToken);
    }

    public async Task DeactivateAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        await SetLoginActiveAsync(studentId, false, cancellationToken);
    }

    public async Task<BulkAuthenticationActivateResultDto> BulkActivateAsync(
        BulkAuthenticationActivateDto dto,
        CancellationToken cancellationToken = default)
    {
        EnsureTenant();
        EnsureCanManage();
        await EnsureReadyAsync(cancellationToken);

        if (dto.StudentIds is null || dto.StudentIds.Count == 0)
            throw new AppException("At least one student id is required.", 400);

        var ids = dto.StudentIds.Distinct().ToList();
        var students = await _unitOfWork.Students.GetByIdsAsync(ids, cancellationToken);
        if (students.Count != ids.Count)
            throw new ForbiddenException("One or more students do not belong to the current tenant.");

        await _unitOfWork.BeginTenantTransactionAsync(cancellationToken);
        try
        {
            foreach (var student in students)
            {
                var user = await _unitOfWork.Users.GetByIdAsync(student.UserId, cancellationToken)
                    ?? throw new NotFoundException($"User account for student '{student.Id}' not found.");

                user.Active = true;
                user.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
            }

            await _unitOfWork.SaveTenantChangesAsync(cancellationToken);
            await _unitOfWork.CommitTenantTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTenantTransactionAsync(cancellationToken);
            throw;
        }

        return new BulkAuthenticationActivateResultDto { Activated = students.Count, Failed = 0 };
    }

    public async Task<(byte[] Content, string ContentType, string FileName)> ExportAsync(
        LoginDeactivateFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        EnsureTenant();
        EnsureCanManage();
        await EnsureReadyAsync(cancellationToken);

        filter.Page = 1;
        filter.PageSize = 10_000;

        var (items, _) = await _unitOfWork.Students.SearchAsync(new StudentSearchFilter
        {
            ClassId = filter.ClassId,
            SectionId = filter.SectionId,
            Search = filter.Search,
            IsActive = null,
            IsLoginActive = false,
            SortBy = "name",
            SortDir = "asc",
            Page = 1,
            PageSize = 10_000
        }, cancellationToken);

        var data = new List<LoginDeactivateListItemDto>(items.Count);
        foreach (var s in items)
            data.Add(await MapItemAsync(s, cancellationToken));

        var fmt = (filter.Export ?? "csv").Trim().ToLowerInvariant();

        return fmt switch
        {
            "csv" or "excel" => BuildCsvExport(data, fmt == "excel"),
            "pdf" => BuildPdfExport(data),
            _ => throw new AppException("Unsupported export format. Use csv, excel, or pdf.", 400)
        };
    }

    private async Task SetLoginActiveAsync(Guid studentId, bool active, CancellationToken cancellationToken)
    {
        EnsureTenant();
        EnsureCanManage();
        await EnsureReadyAsync(cancellationToken);

        var student = await _unitOfWork.Students.GetByIdAsync(studentId, cancellationToken)
            ?? throw new NotFoundException($"Student '{studentId}' not found.");

        var user = await _unitOfWork.Users.GetByIdAsync(student.UserId, cancellationToken)
            ?? throw new NotFoundException($"User account for student '{studentId}' not found.");

        user.Active = active;
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveTenantChangesAsync(cancellationToken);
    }

    private async Task<LoginDeactivateListItemDto> MapItemAsync(Student s, CancellationToken cancellationToken)
    {
        var primary = s.Guardians.FirstOrDefault(g => g.IsPrimary) ?? s.Guardians.FirstOrDefault();
        var reason = s.DeactivateReasonRef?.Reason ?? s.DeactivateReason;

        return new LoginDeactivateListItemDto
        {
            Id = s.Id,
            PhotoUrl = await PresignAsync(s.ProfilePictureUrl, cancellationToken),
            Name = string.IsNullOrWhiteSpace(s.LastName) ? s.FirstName : $"{s.FirstName} {s.LastName}",
            RegisterNo = s.RegisterNo,
            Roll = s.Roll,
            GuardianName = primary?.Name,
            ClassName = s.Class?.Name ?? string.Empty,
            DeactivateReason = reason,
            Email = s.Email,
            MobileNo = s.MobileNo,
            IsLoginActive = s.User?.Active ?? false
        };
    }

    private async Task<string?> PresignAsync(string? objectKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(objectKey))
            return null;

        var slug = _tenantContext.TenantSlug;
        if (string.IsNullOrWhiteSpace(slug))
            return objectKey;

        try
        {
            return await _storageService.GetPresignedUrlAsync(slug, objectKey, cancellationToken);
        }
        catch
        {
            return objectKey;
        }
    }

    private static (byte[] Content, string ContentType, string FileName) BuildCsvExport(
        List<LoginDeactivateListItemDto> items, bool asExcel)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Name,RegisterNo,Roll,GuardianName,Class,DeactivateReason,Email,MobileNo,IsLoginActive");
        foreach (var s in items)
        {
            sb.AppendLine(string.Join(',',
                Csv(s.Name),
                Csv(s.RegisterNo),
                Csv(s.Roll),
                Csv(s.GuardianName),
                Csv(s.ClassName),
                Csv(s.DeactivateReason),
                Csv(s.Email),
                Csv(s.MobileNo),
                s.IsLoginActive ? "Yes" : "No"));
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return asExcel
            ? (bytes, "application/vnd.ms-excel", $"login-deactivate-{DateTime.UtcNow:yyyyMMdd}.xls")
            : (bytes, "text/csv", $"login-deactivate-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    private static string Csv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return $"\"{value.Replace("\"", "\"\"")}\"";
    }

    private (byte[] Content, string ContentType, string FileName) BuildPdfExport(List<LoginDeactivateListItemDto> items)
    {
        var school = _tenantContext.TenantName ?? "School";
        var lines = new List<string> { school, "Login Deactivate Export", "" };
        foreach (var s in items)
            lines.Add($"{s.RegisterNo} | {s.Name} | {s.ClassName} | {s.DeactivateReason}");

        var content = string.Join('\n', lines)
            .Replace("\\", "\\\\")
            .Replace("(", "\\(")
            .Replace(")", "\\)");

        var pdf = $"""
            %PDF-1.4
            1 0 obj<< /Type /Catalog /Pages 2 0 R >>endobj
            2 0 obj<< /Type /Pages /Kids [3 0 R] /Count 1 >>endobj
            3 0 obj<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources<< /Font<< /F1 5 0 R >> >> >>endobj
            4 0 obj<< /Length {content.Length + 50} >>stream
            BT /F1 10 Tf 40 750 Td 14 TL ({content.Replace("\n", ") Tj T* (")}) Tj ET
            endstream endobj
            5 0 obj<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>endobj
            xref
            0 6
            0000000000 65535 f 
            trailer<< /Size 6 /Root 1 0 R >>
            startxref
            0
            %%EOF
            """;

        return (Encoding.ASCII.GetBytes(pdf), "application/pdf", $"login-deactivate-{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    private async Task EnsureReadyAsync(CancellationToken cancellationToken)
    {
        await _schemaProvisioner.EnsureAdmissionModuleAsync(_tenantContext.SchemaName!, cancellationToken);
        await _schemaProvisioner.EnsureDeactivateReasonMasterAsync(_tenantContext.SchemaName!, cancellationToken);
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
        throw new ForbiddenException("Only Super Admin or School Admin can manage login deactivation.");
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
