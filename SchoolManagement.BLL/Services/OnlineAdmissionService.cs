using System.Globalization;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SchoolManagement.BLL.DTOs.OnlineAdmission;
using SchoolManagement.BLL.DTOs.Student;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.BLL.Mappings;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Master;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class OnlineAdmissionService : IOnlineAdmissionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISchoolRepository _schoolRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ITenantSchemaProvisioner _schemaProvisioner;
    private readonly IStudentService _studentService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<OnlineAdmissionService> _logger;
    private static readonly Random Rng = Random.Shared;

    public OnlineAdmissionService(
        IUnitOfWork unitOfWork,
        ISchoolRepository schoolRepository,
        ITenantContext tenantContext,
        ITenantSchemaProvisioner schemaProvisioner,
        IStudentService studentService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<OnlineAdmissionService> logger)
    {
        _unitOfWork = unitOfWork;
        _schoolRepository = schoolRepository;
        _tenantContext = tenantContext;
        _schemaProvisioner = schemaProvisioner;
        _studentService = studentService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<OnlineAdmissionResponseDto> ApplyAsync(
        SubmitOnlineAdmissionDto dto,
        CancellationToken cancellationToken = default)
    {
        var tenant = await ResolveTenantBySlugAsync(dto.TenantSlug, cancellationToken);
        SetTenant(tenant);
        await EnsureReadyAsync(cancellationToken);

        var clazz = await _unitOfWork.AdmissionLookups.GetClassByIdAsync(dto.ClassId, cancellationToken)
            ?? throw new AppException("Invalid ClassId for this school.", 400);

        var referenceNo = await GenerateUniqueReferenceNoAsync(cancellationToken);
        var now = DateTime.UtcNow;

        var entity = new OnlineAdmission
        {
            Id = Guid.NewGuid(),
            ReferenceNo = referenceNo,
            AcademicYear = dto.AcademicYear,
            ClassId = clazz.Id,
            ClassName = clazz.Name,
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName?.Trim(),
            Gender = dto.Gender,
            DateOfBirth = dto.DateOfBirth?.Date,
            BloodGroup = dto.BloodGroup,
            Religion = dto.Religion,
            MobileNo = dto.MobileNo.Trim(),
            Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim().ToLowerInvariant(),
            PresentAddress = dto.PresentAddress,
            PermanentAddress = dto.PermanentAddress,
            BirthRegistrationNumber = dto.BirthRegistrationNumber,
            GuardianName = dto.GuardianName,
            GuardianRelation = dto.GuardianRelation,
            GuardianMobile = dto.GuardianMobile,
            GuardianEmail = string.IsNullOrWhiteSpace(dto.GuardianEmail) ? null : dto.GuardianEmail.Trim().ToLowerInvariant(),
            FatherName = dto.FatherName,
            MotherName = dto.MotherName,
            PreviousSchoolName = dto.PreviousSchoolName,
            PreviousSchoolQualification = dto.PreviousSchoolQualification,
            Status = OnlineAdmissionStatuses.Apply,
            PaymentStatus = OnlineAdmissionPaymentStatuses.Unpaid,
            ApplyDate = now,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _unitOfWork.OnlineAdmissions.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveTenantChangesAsync(cancellationToken);

        _logger.LogInformation("Online admission {Ref} submitted for tenant {Slug}", referenceNo, tenant.Slug);
        return MapToDto(entity, 1);
    }

    public async Task<OnlineAdmissionTrackDto> TrackAsync(
        string referenceNo,
        string? tenantSlug = null,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(tenantSlug))
        {
            var tenant = await ResolveTenantBySlugAsync(tenantSlug, cancellationToken);
            SetTenant(tenant);
        }
        else if (string.IsNullOrEmpty(_tenantContext.SchemaName))
        {
            throw new AppException("Provide ?school={slug} or X-Tenant-ID to track an application.", 400);
        }

        await EnsureReadyAsync(cancellationToken);

        var entity = await _unitOfWork.OnlineAdmissions.GetByReferenceNoAsync(referenceNo.Trim(), cancellationToken)
            ?? throw new NotFoundException($"Application '{referenceNo}' not found.");

        return new OnlineAdmissionTrackDto
        {
            ReferenceNo = entity.ReferenceNo,
            Name = FullName(entity),
            ClassName = entity.ClassName ?? entity.Class?.Name,
            Status = entity.Status,
            PaymentStatus = entity.PaymentStatus,
            ApplyDate = entity.ApplyDate
        };
    }

    public async Task<IReadOnlyList<AdmissionLookupItemDto>> GetPublicClassesAsync(
        string tenantSlug,
        CancellationToken cancellationToken = default)
    {
        var tenant = await ResolveTenantBySlugAsync(tenantSlug, cancellationToken);
        SetTenant(tenant);
        await EnsureReadyAsync(cancellationToken);

        var classes = await _unitOfWork.AdmissionLookups.GetClassesAsync(cancellationToken);
        return classes.Select(AdmissionMappings.ToLookup).ToList();
    }

    public async Task<OnlineAdmissionListResponseDto> GetListAsync(
        OnlineAdmissionFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        EnsureAdminTenant();
        await EnsureReadyAsync(cancellationToken);
        EnsureCanRead();

        var search = new OnlineAdmissionSearchFilter
        {
            ClassId = filter.ClassId,
            Status = filter.Status,
            PaymentStatus = filter.PaymentStatus,
            AcademicYear = filter.AcademicYear,
            Search = filter.Search,
            Page = filter.Page,
            PageSize = filter.PageSize
        };

        var (items, total) = await _unitOfWork.OnlineAdmissions.SearchAsync(search, cancellationToken);
        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize is < 1 or > 200 ? 10 : filter.PageSize;
        var start = (page - 1) * pageSize;

        return new OnlineAdmissionListResponseDto
        {
            Items = items.Select((o, i) => MapToDto(o, start + i + 1)).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            TotalPages = pageSize <= 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize)
        };
    }

    public async Task<OnlineAdmissionResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EnsureAdminTenant();
        await EnsureReadyAsync(cancellationToken);
        EnsureCanRead();

        var entity = await _unitOfWork.OnlineAdmissions.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Online admission '{id}' not found.");
        return MapToDto(entity, 1);
    }

    public async Task<OnlineAdmissionResponseDto> ApproveAsync(
        Guid id,
        ApproveAdmissionDto dto,
        CancellationToken cancellationToken = default)
    {
        EnsureAdminTenant();
        await EnsureReadyAsync(cancellationToken);
        EnsureCanManage();

        var entity = await _unitOfWork.OnlineAdmissions.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Online admission '{id}' not found.");

        if (entity.Status != OnlineAdmissionStatuses.Apply)
            throw new AppException($"Only applications in '{OnlineAdmissionStatuses.Apply}' status can be approved. Current: {entity.Status}", 400);

        await _unitOfWork.BeginTenantTransactionAsync(cancellationToken);
        try
        {
            var student = await _studentService.CreateFromOnlineAdmissionAsync(entity, dto, cancellationToken);

            entity.Status = OnlineAdmissionStatuses.Approved;
            entity.StudentId = student.Id;
            entity.ReviewedBy = GetCurrentUserId();
            entity.ReviewedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.OnlineAdmissions.UpdateAsync(entity, cancellationToken);

            await _unitOfWork.SaveTenantChangesAsync(cancellationToken);
            await _unitOfWork.CommitTenantTransactionAsync(cancellationToken);

            _logger.LogInformation("Online admission {Ref} approved → student {StudentId}", entity.ReferenceNo, student.Id);
            return MapToDto(entity, 1);
        }
        catch
        {
            await _unitOfWork.RollbackTenantTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<OnlineAdmissionResponseDto> DeclineAsync(
        Guid id,
        DeclineAdmissionDto dto,
        CancellationToken cancellationToken = default)
    {
        EnsureAdminTenant();
        await EnsureReadyAsync(cancellationToken);
        EnsureCanManage();

        var entity = await _unitOfWork.OnlineAdmissions.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Online admission '{id}' not found.");

        if (entity.Status != OnlineAdmissionStatuses.Apply)
            throw new AppException($"Only applications in '{OnlineAdmissionStatuses.Apply}' status can be declined. Current: {entity.Status}", 400);

        entity.Status = OnlineAdmissionStatuses.Declined;
        entity.DeclineReason = dto.Reason;
        entity.ReviewedBy = GetCurrentUserId();
        entity.ReviewedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.OnlineAdmissions.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveTenantChangesAsync(cancellationToken);
        return MapToDto(entity, 1);
    }

    public async Task<OnlineAdmissionResponseDto> UpdatePaymentAsync(
        Guid id,
        UpdatePaymentStatusDto dto,
        CancellationToken cancellationToken = default)
    {
        EnsureAdminTenant();
        await EnsureReadyAsync(cancellationToken);
        EnsureCanManage();

        var entity = await _unitOfWork.OnlineAdmissions.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Online admission '{id}' not found.");

        entity.PaymentStatus = dto.PaymentStatus.Trim();
        if (dto.PaymentAmount.HasValue) entity.PaymentAmount = dto.PaymentAmount;
        if (dto.PaymentReference is not null) entity.PaymentReference = dto.PaymentReference;
        entity.PaymentDate = dto.PaymentDate ?? (entity.PaymentStatus == OnlineAdmissionPaymentStatuses.Paid
            ? DateTime.UtcNow
            : entity.PaymentDate);
        if (entity.PaymentStatus == OnlineAdmissionPaymentStatuses.Unpaid)
            entity.PaymentDate = null;

        entity.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.OnlineAdmissions.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveTenantChangesAsync(cancellationToken);
        return MapToDto(entity, 1);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EnsureAdminTenant();
        await EnsureReadyAsync(cancellationToken);
        EnsureCanManage();

        var entity = await _unitOfWork.OnlineAdmissions.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Online admission '{id}' not found.");

        if (entity.Status == OnlineAdmissionStatuses.Approved)
            throw new AppException("Approved applications cannot be deleted.", 400);

        await _unitOfWork.OnlineAdmissions.DeleteAsync(entity, cancellationToken);
        await _unitOfWork.SaveTenantChangesAsync(cancellationToken);
    }

    public Task<OnlineAdmissionResponseDto> GetPrintDataAsync(Guid id, CancellationToken cancellationToken = default)
        => GetByIdAsync(id, cancellationToken);

    public async Task<(byte[] Content, string ContentType, string FileName)> ExportAsync(
        OnlineAdmissionFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        EnsureAdminTenant();
        await EnsureReadyAsync(cancellationToken);
        EnsureCanRead();

        var exportFilter = new OnlineAdmissionSearchFilter
        {
            ClassId = filter.ClassId,
            Status = filter.Status,
            PaymentStatus = filter.PaymentStatus,
            AcademicYear = filter.AcademicYear,
            Search = filter.Search,
            Page = 1,
            PageSize = 10_000
        };

        var (items, _) = await _unitOfWork.OnlineAdmissions.SearchAsync(exportFilter, cancellationToken);
        var fmt = (filter.Export ?? "csv").Trim().ToLowerInvariant();

        return fmt switch
        {
            "csv" or "excel" => BuildCsvExport(items, fmt == "excel"),
            "pdf" => BuildPdfExport(items),
            _ => throw new AppException("Unsupported export format. Use csv, excel, or pdf.", 400)
        };
    }

    private async Task EnsureReadyAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_tenantContext.SchemaName))
            throw new AppException("Tenant schema is not resolved.", 400);

        await _schemaProvisioner.EnsureAdmissionModuleAsync(_tenantContext.SchemaName, cancellationToken);
        await _schemaProvisioner.EnsureOnlineAdmissionModuleAsync(_tenantContext.SchemaName, cancellationToken);
    }

    private async Task<Tenant> ResolveTenantBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(slug))
            throw new AppException("School slug is required.", 400);

        var tenant = await _schoolRepository.GetBySlugAsync(slug.Trim().ToLowerInvariant(), cancellationToken)
            ?? throw new NotFoundException($"School '{slug}' not found.");

        if (!tenant.IsActive)
            throw new AppException($"School '{slug}' is inactive.", 403);

        return tenant;
    }

    private void SetTenant(Tenant tenant)
    {
        _tenantContext.SetTenant(tenant.Id, tenant.Slug, tenant.SchemaName, tenant.Name);
    }

    private void EnsureAdminTenant()
    {
        if (string.IsNullOrEmpty(_tenantContext.SchemaName))
            throw new AppException("X-Tenant-ID header is required.", 400);
    }

    private async Task<string> GenerateUniqueReferenceNoAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 20; attempt++)
        {
            var candidate = Rng.Next(10_000_000, 99_999_999).ToString(CultureInfo.InvariantCulture);
            if (!await _unitOfWork.OnlineAdmissions.ReferenceNoExistsAsync(candidate, cancellationToken))
                return candidate;
        }

        throw new AppException("Could not generate a unique reference number. Please retry.", 500);
    }

    private void EnsureCanManage()
    {
        var roles = GetCurrentRoles();
        if (roles.Contains(AppConstants.Roles.SuperAdmin) || roles.Contains(AppConstants.Roles.Admin))
            return;
        throw new ForbiddenException("Only Super Admin or School Admin can manage online admissions.");
    }

    private void EnsureCanRead()
    {
        var roles = GetCurrentRoles();
        if (roles.Contains(AppConstants.Roles.SuperAdmin) ||
            roles.Contains(AppConstants.Roles.Admin) ||
            roles.Contains(AppConstants.Roles.Teacher))
            return;
        throw new ForbiddenException("You do not have access to online admissions.");
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

    private static string FullName(OnlineAdmission o) =>
        string.IsNullOrWhiteSpace(o.LastName) ? o.FirstName : $"{o.FirstName} {o.LastName}";

    private static OnlineAdmissionResponseDto MapToDto(OnlineAdmission o, int sl) => new()
    {
        Id = o.Id,
        Sl = sl,
        ReferenceNo = o.ReferenceNo,
        Name = FullName(o),
        FirstName = o.FirstName,
        LastName = o.LastName,
        Gender = o.Gender,
        DateOfBirth = o.DateOfBirth,
        BloodGroup = o.BloodGroup,
        Religion = o.Religion,
        ClassId = o.ClassId,
        ClassName = o.ClassName ?? o.Class?.Name,
        AcademicYear = o.AcademicYear,
        MobileNo = o.MobileNo,
        Email = o.Email,
        PresentAddress = o.PresentAddress,
        PermanentAddress = o.PermanentAddress,
        BirthRegistrationNumber = o.BirthRegistrationNumber,
        ProfilePictureUrl = o.ProfilePictureUrl,
        GuardianName = o.GuardianName,
        GuardianRelation = o.GuardianRelation,
        GuardianMobile = o.GuardianMobile,
        GuardianEmail = o.GuardianEmail,
        FatherName = o.FatherName,
        MotherName = o.MotherName,
        PreviousSchoolName = o.PreviousSchoolName,
        PreviousSchoolQualification = o.PreviousSchoolQualification,
        Status = o.Status,
        PaymentStatus = o.PaymentStatus,
        PaymentAmount = o.PaymentAmount,
        PaymentDate = o.PaymentDate,
        PaymentReference = o.PaymentReference,
        ApplyDate = o.ApplyDate,
        DeclineReason = o.DeclineReason,
        StudentId = o.StudentId,
        ReviewedBy = o.ReviewedBy,
        ReviewedAt = o.ReviewedAt
    };

    private static (byte[] Content, string ContentType, string FileName) BuildCsvExport(
        IReadOnlyList<OnlineAdmission> items, bool asExcel)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Sl,ReferenceNo,Name,Class,Mobile,Status,PaymentStatus,ApplyDate");
        for (var i = 0; i < items.Count; i++)
        {
            var o = items[i];
            sb.AppendLine(string.Join(',',
                i + 1,
                Csv(o.ReferenceNo),
                Csv(FullName(o)),
                Csv(o.ClassName),
                Csv(o.MobileNo),
                Csv(o.Status),
                Csv(o.PaymentStatus),
                o.ApplyDate.ToString("u", CultureInfo.InvariantCulture)));
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return asExcel
            ? (bytes, "application/vnd.ms-excel", $"online-admissions-{DateTime.UtcNow:yyyyMMdd}.xls")
            : (bytes, "text/csv", $"online-admissions-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    private static string Csv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return $"\"{value.Replace("\"", "\"\"")}\"";
    }

    private static (byte[] Content, string ContentType, string FileName) BuildPdfExport(
        IReadOnlyList<OnlineAdmission> items)
    {
        var lines = new List<string> { "Online Admissions Export", "" };
        for (var i = 0; i < items.Count; i++)
        {
            var o = items[i];
            lines.Add($"{i + 1}. {o.ReferenceNo} — {FullName(o)} — {o.Status} / {o.PaymentStatus}");
        }

        var content = string.Join('\n', lines)
            .Replace("\\", "\\\\")
            .Replace("(", "\\(")
            .Replace(")", "\\)");

        var stream = new MemoryStream();
        var y = 750;
        var textOps = new StringBuilder("BT /F1 10 Tf 50 750 Td\n");
        foreach (var line in content.Split('\n'))
        {
            textOps.Append($"({line.Replace("\r", "")}) Tj\n0 -14 Td\n");
            y -= 14;
            if (y < 50) break;
        }
        textOps.Append("ET");

        var contentStream = textOps.ToString();
        var pdf = new StringBuilder();
        pdf.Append("%PDF-1.4\n");
        var offsets = new List<long>();

        void Obj(int id, string body)
        {
            offsets.Add(Encoding.ASCII.GetByteCount(pdf.ToString()));
            pdf.Append(id).Append(" 0 obj\n").Append(body).Append("\nendobj\n");
        }

        Obj(1, "<< /Type /Catalog /Pages 2 0 R >>");
        Obj(2, "<< /Type /Pages /Kids [3 0 R] /Count 1 >>");
        Obj(3, "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>");
        Obj(4, $"<< /Length {contentStream.Length} >>\nstream\n{contentStream}\nendstream");
        Obj(5, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");

        var xref = Encoding.ASCII.GetByteCount(pdf.ToString());
        pdf.Append($"xref\n0 {offsets.Count + 1}\n0000000000 65535 f \n");
        foreach (var off in offsets)
            pdf.Append(off.ToString("D10", CultureInfo.InvariantCulture)).Append(" 00000 n \n");
        pdf.Append($"trailer\n<< /Size {offsets.Count + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF");

        return (Encoding.ASCII.GetBytes(pdf.ToString()), "application/pdf",
            $"online-admissions-{DateTime.UtcNow:yyyyMMdd}.pdf");
    }
}
