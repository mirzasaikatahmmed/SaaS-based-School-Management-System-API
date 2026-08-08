using System.Globalization;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.Student;
using SchoolManagement.BLL.DTOs.StudentList;
using StudentListPageDto = SchoolManagement.BLL.DTOs.StudentList.StudentListResponseDto;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class StudentListService : IStudentListService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ITenantSchemaProvisioner _schemaProvisioner;
    private readonly IStorageService _storageService;
    private readonly IStudentService _studentService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public StudentListService(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ITenantSchemaProvisioner schemaProvisioner,
        IStorageService storageService,
        IStudentService studentService,
        IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _schemaProvisioner = schemaProvisioner;
        _storageService = storageService;
        _studentService = studentService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<StudentListPageDto> GetListAsync(
        StudentListFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        await EnsureReadyAsync(cancellationToken);
        EnsureCanReadList();

        if (!filter.ClassId.HasValue)
        {
            return EmptyResponse(filter, "Class is required for filtering.");
        }

        return await QueryListAsync(filter, isActive: true, cancellationToken: cancellationToken);
    }

    public async Task<StudentListPageDto> GetLoginDeactivateListAsync(
        StudentListFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        await EnsureReadyAsync(cancellationToken);
        EnsureCanManage();

        if (!filter.ClassId.HasValue)
            return EmptyResponse(filter, "Class is required for filtering.");

        return await QueryListAsync(filter, isActive: true, cancellationToken: cancellationToken);
    }

    public async Task<StudentListPageDto> GetDeactivateReasonsAsync(
        StudentListFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        await EnsureReadyAsync(cancellationToken);
        EnsureCanManage();
        return await QueryListAsync(filter, isActive: false, requireClass: false, cancellationToken: cancellationToken);
    }

    public async Task<StudentDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await EnsureReadyAsync(cancellationToken);
        var student = await _unitOfWork.Students.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Student '{id}' not found.");

        EnsureCanReadStudent(student);
        return await MapDetailAsync(student, cancellationToken);
    }

    public async Task<StudentDetailDto> GetMeAsync(CancellationToken cancellationToken = default)
    {
        await EnsureReadyAsync(cancellationToken);
        var roles = GetCurrentRoles();
        if (!roles.Contains(AppConstants.Roles.Student))
            throw new ForbiddenException("Only students can access this endpoint.");

        var userId = GetCurrentUserId();
        var student = await _unitOfWork.Students.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Student profile not found for the current user.");

        return await MapDetailAsync(student, cancellationToken);
    }

    public async Task<StudentDetailDto> UpdateAsync(
        Guid id,
        UpdateAdmissionDto dto,
        CancellationToken cancellationToken = default)
    {
        await EnsureReadyAsync(cancellationToken);
        EnsureCanManage();
        await _studentService.UpdateAdmissionAsync(id, dto, cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await EnsureReadyAsync(cancellationToken);
        EnsureCanManage();

        var student = await _unitOfWork.Students.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Student '{id}' not found.");

        SoftDeactivate(student, reason: null, reasonId: null);
        await _unitOfWork.Students.UpdateAsync(student, cancellationToken);
        await _unitOfWork.SaveTenantChangesAsync(cancellationToken);
    }

    public async Task<BulkDeleteResultDto> BulkDeleteAsync(
        BulkDeleteDto dto,
        CancellationToken cancellationToken = default)
    {
        await EnsureReadyAsync(cancellationToken);
        EnsureCanManage();

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
                SoftDeactivate(student, reason: null, reasonId: null);
                await _unitOfWork.Students.UpdateAsync(student, cancellationToken);
            }

            await _unitOfWork.SaveTenantChangesAsync(cancellationToken);
            await _unitOfWork.CommitTenantTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTenantTransactionAsync(cancellationToken);
            throw;
        }

        return new BulkDeleteResultDto { Deleted = students.Count, Failed = 0 };
    }

    public async Task<(byte[] Content, string ContentType, string FileName)> ExportAsync(
        StudentListFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        await EnsureReadyAsync(cancellationToken);
        EnsureCanManage();

        if (!filter.ClassId.HasValue)
            throw new AppException("Class is required for export.", 400);

        filter.Page = 1;
        filter.PageSize = 10_000;
        var list = await QueryListAsync(filter, isActive: true, cancellationToken: cancellationToken);
        var fmt = (filter.Export ?? "csv").Trim().ToLowerInvariant();

        return fmt switch
        {
            "csv" or "excel" => BuildCsvExport(list.Data, fmt == "excel"),
            "pdf" => BuildPdfExport(list.Data),
            _ => throw new AppException("Unsupported export format. Use csv, excel, or pdf.", 400)
        };
    }

    public async Task ToggleLoginAsync(
        Guid id,
        LoginDeactivateDto dto,
        CancellationToken cancellationToken = default)
    {
        await EnsureReadyAsync(cancellationToken);
        EnsureCanManage();

        var student = await _unitOfWork.Students.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Student '{id}' not found.");

        var user = await _unitOfWork.Users.GetByIdAsync(student.UserId, cancellationToken)
            ?? throw new NotFoundException($"User account for student '{id}' not found.");

        user.Active = dto.IsLoginActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveTenantChangesAsync(cancellationToken);
    }

    public async Task DeactivateAsync(
        Guid id,
        DeactivateReasonDto dto,
        CancellationToken cancellationToken = default)
    {
        await EnsureReadyAsync(cancellationToken);
        EnsureCanManage();

        if (string.IsNullOrWhiteSpace(dto.Reason))
            throw new AppException("Deactivation reason is required.", 400);

        var student = await _unitOfWork.Students.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Student '{id}' not found.");

        var reasonText = dto.Reason.Trim();
        Guid? reasonId = null;
        try
        {
            await _schemaProvisioner.EnsureDeactivateReasonMasterAsync(_tenantContext.SchemaName!, cancellationToken);
            var master = await _unitOfWork.DeactivateReasons.GetByReasonAsync(reasonText, cancellationToken);
            if (master is not null)
                reasonId = master.Id;
        }
        catch
        {
            // master table may not exist yet — keep free-text reason
        }

        SoftDeactivate(student, reasonText, reasonId);
        await _unitOfWork.Students.UpdateAsync(student, cancellationToken);
        await _unitOfWork.SaveTenantChangesAsync(cancellationToken);
    }

    public async Task ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await EnsureReadyAsync(cancellationToken);
        EnsureCanManage();

        var student = await _unitOfWork.Students.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Student '{id}' not found.");

        student.IsActive = true;
        student.DeactivateReason = null;
        student.DeactivateReasonId = null;
        student.DeactivatedAt = null;
        student.DeactivatedBy = null;
        student.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Students.UpdateAsync(student, cancellationToken);
        await _unitOfWork.SaveTenantChangesAsync(cancellationToken);
    }

    private async Task<StudentListPageDto> QueryListAsync(
        StudentListFilterDto filter,
        bool? isActive,
        CancellationToken cancellationToken = default,
        bool requireClass = true)
    {
        if (requireClass && !filter.ClassId.HasValue)
            return EmptyResponse(filter);

        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize is < 1 or > 200 ? 20 : filter.PageSize;
        var academicYear = filter.AcademicYear ?? DateTime.Today.Year;

        var searchFilter = new StudentSearchFilter
        {
            ClassId = filter.ClassId,
            SectionId = filter.SectionId,
            AcademicYear = academicYear,
            Search = filter.Search,
            IsActive = isActive,
            SortBy = filter.SortBy,
            SortDir = filter.SortDir,
            Page = page,
            PageSize = pageSize
        };

        // Deactivate-reason list may omit class; still default academic year unless searching all years
        if (!requireClass && !filter.ClassId.HasValue && !filter.AcademicYear.HasValue)
            searchFilter.AcademicYear = null;

        var (items, total) = await _unitOfWork.Students.SearchAsync(searchFilter, cancellationToken);
        var data = new List<StudentListItemDto>(items.Count);
        foreach (var s in items)
            data.Add(await MapListItemAsync(s, cancellationToken));

        return new StudentListPageDto
        {
            Data = data,
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize)
        };
    }

    private void SoftDeactivate(Student student, string? reason, Guid? reasonId)
    {
        student.IsActive = false;
        student.DeactivateReason = reason;
        student.DeactivateReasonId = reasonId;
        student.DeactivatedAt = DateTime.UtcNow;
        student.DeactivatedBy = GetCurrentUserId();
        student.UpdatedAt = DateTime.UtcNow;
    }

    private async Task<StudentListItemDto> MapListItemAsync(Student s, CancellationToken cancellationToken)
    {
        var father = s.Guardians.FirstOrDefault(g =>
            g.Relation.Contains("father", StringComparison.OrdinalIgnoreCase));
        var mother = s.Guardians.FirstOrDefault(g =>
            g.Relation.Contains("mother", StringComparison.OrdinalIgnoreCase));
        var primary = s.Guardians.FirstOrDefault(g => g.IsPrimary) ?? s.Guardians.FirstOrDefault();

        return new StudentListItemDto
        {
            Id = s.Id,
            PhotoUrl = await PresignAsync(s.ProfilePictureUrl, cancellationToken),
            Name = FullName(s.FirstName, s.LastName),
            ClassName = s.Class?.Name ?? string.Empty,
            SectionName = s.Section?.Name ?? string.Empty,
            RegisterNo = s.RegisterNo,
            Roll = s.Roll,
            DateOfBirth = FormatDob(s.DateOfBirth),
            Age = CalculateAge(s.DateOfBirth),
            Gender = s.Gender,
            FatherName = father?.Name,
            MotherName = mother?.Name,
            GuardianMobileNo = primary?.MobileNo,
            IsActive = s.IsActive,
            IsLoginActive = s.User?.Active ?? false,
            DeactivateReason = s.DeactivateReasonRef?.Reason ?? s.DeactivateReason,
            DeactivatedAt = s.DeactivatedAt
        };
    }

    private async Task<StudentDetailDto> MapDetailAsync(Student s, CancellationToken cancellationToken)
    {
        var guardians = new List<GuardianDetailDto>();
        foreach (var g in s.Guardians.OrderByDescending(x => x.IsPrimary).ThenBy(x => x.CreatedAt))
        {
            guardians.Add(new GuardianDetailDto
            {
                Id = g.Id,
                Name = g.Name,
                Relation = g.Relation,
                FatherName = g.FatherName,
                MotherName = g.MotherName,
                MobileNo = g.MobileNo,
                Email = g.Email,
                Occupation = g.Occupation,
                Address = g.Address,
                PhotoUrl = await PresignAsync(g.ProfilePictureUrl, cancellationToken),
                IsPrimary = g.IsPrimary
            });
        }

        return new StudentDetailDto
        {
            Id = s.Id,
            RegisterNo = s.RegisterNo,
            Roll = s.Roll,
            AcademicYear = s.AcademicYear,
            AdmissionDate = s.AdmissionDate,
            ClassName = s.Class?.Name ?? string.Empty,
            SectionName = s.Section?.Name ?? string.Empty,
            CategoryName = s.Category?.Name,
            FirstName = s.FirstName,
            LastName = s.LastName,
            Gender = s.Gender,
            BloodGroup = s.BloodGroup,
            DateOfBirth = FormatDob(s.DateOfBirth),
            Age = CalculateAge(s.DateOfBirth),
            MotherTongue = s.MotherTongue,
            Religion = s.Religion,
            Caste = s.Caste,
            MobileNo = s.MobileNo,
            Email = s.Email,
            City = s.City,
            State = s.State,
            PresentAddress = s.PresentAddress,
            PermanentAddress = s.PermanentAddress,
            FathersNidNumber = s.FathersNidNumber,
            MothersNidNumber = s.MothersNidNumber,
            BirthRegistrationNumber = s.BirthRegistrationNumber,
            PhotoUrl = await PresignAsync(s.ProfilePictureUrl, cancellationToken),
            Username = s.User?.Username ?? string.Empty,
            IsLoginActive = s.User?.Active ?? false,
            Guardians = guardians,
            TransportRoute = s.TransportRoute?.Name,
            VehicleNo = s.VehicleNo,
            HostelName = s.Hostel?.Name,
            RoomName = s.Room?.Name,
            PreviousSchoolName = s.PreviousSchoolName,
            PreviousSchoolQualification = s.PreviousSchoolQualification,
            Remarks = s.Remarks,
            IsActive = s.IsActive,
            DeactivateReason = s.DeactivateReasonRef?.Reason ?? s.DeactivateReason,
            CreatedAt = s.CreatedAt
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

    private static int? CalculateAge(DateTime? dateOfBirth)
    {
        if (!dateOfBirth.HasValue) return null;
        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Value.Year;
        if (dateOfBirth.Value.Date > today.AddYears(-age))
            age--;
        return age < 0 ? 0 : age;
    }

    private static string? FormatDob(DateTime? dob) =>
        dob?.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);

    private static string FullName(string first, string? last) =>
        string.IsNullOrWhiteSpace(last) ? first : $"{first} {last}";

    private static StudentListPageDto EmptyResponse(StudentListFilterDto filter, string? _ = null)
    {
        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize is < 1 or > 200 ? 20 : filter.PageSize;
        return new StudentListPageDto
        {
            Data = [],
            TotalCount = 0,
            Page = page,
            PageSize = pageSize,
            TotalPages = 0
        };
    }

    private static (byte[] Content, string ContentType, string FileName) BuildCsvExport(
        List<StudentListItemDto> items, bool asExcel)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Photo,Name,Class,Section,RegisterNo,Roll,DateOfBirth,Age,Gender,FatherName,MotherName,GuardianMobile,IsActive,IsLoginActive");
        foreach (var s in items)
        {
            sb.AppendLine(string.Join(',',
                Csv(s.PhotoUrl),
                Csv(s.Name),
                Csv(s.ClassName),
                Csv(s.SectionName),
                Csv(s.RegisterNo),
                Csv(s.Roll),
                Csv(s.DateOfBirth),
                s.Age?.ToString(CultureInfo.InvariantCulture) ?? "",
                Csv(s.Gender),
                Csv(s.FatherName),
                Csv(s.MotherName),
                Csv(s.GuardianMobileNo),
                s.IsActive ? "Yes" : "No",
                s.IsLoginActive ? "Yes" : "No"));
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return asExcel
            ? (bytes, "application/vnd.ms-excel", $"students-{DateTime.UtcNow:yyyyMMdd}.xls")
            : (bytes, "text/csv", $"students-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    private static string Csv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return $"\"{value.Replace("\"", "\"\"")}\"";
    }

    private (byte[] Content, string ContentType, string FileName) BuildPdfExport(List<StudentListItemDto> items)
    {
        var school = _tenantContext.TenantName ?? "School";
        var lines = new List<string> { school, "Student List Export", "" };
        foreach (var s in items)
        {
            lines.Add($"{s.RegisterNo} | {s.Name} | {s.ClassName}-{s.SectionName} | Roll:{s.Roll} | Age:{s.Age} | {s.Gender}");
        }

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

        return (Encoding.ASCII.GetBytes(pdf), "application/pdf", $"students-{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    private async Task EnsureReadyAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_tenantContext.SchemaName))
            throw new AppException("X-Tenant-ID header is required.", 400);

        await _schemaProvisioner.EnsureAdmissionModuleAsync(_tenantContext.SchemaName, cancellationToken);
        await _schemaProvisioner.EnsureStudentDeactivationFieldsAsync(_tenantContext.SchemaName, cancellationToken);
    }

    private void EnsureCanReadList()
    {
        var roles = GetCurrentRoles();
        if (roles.Contains(AppConstants.Roles.SuperAdmin) ||
            roles.Contains(AppConstants.Roles.Admin) ||
            roles.Contains(AppConstants.Roles.Teacher))
            return;
        throw new ForbiddenException("You do not have access to the student list.");
    }

    private void EnsureCanManage()
    {
        var roles = GetCurrentRoles();
        if (roles.Contains(AppConstants.Roles.SuperAdmin) || roles.Contains(AppConstants.Roles.Admin))
            return;
        throw new ForbiddenException("Only Super Admin or School Admin can perform this action.");
    }

    private void EnsureCanReadStudent(Student student)
    {
        var roles = GetCurrentRoles();
        if (roles.Contains(AppConstants.Roles.SuperAdmin) ||
            roles.Contains(AppConstants.Roles.Admin) ||
            roles.Contains(AppConstants.Roles.Teacher))
            return;

        var userId = GetCurrentUserId();
        if (roles.Contains(AppConstants.Roles.Student) && student.UserId == userId)
            return;

        if (roles.Contains(AppConstants.Roles.Parent) &&
            student.Guardians.Any(g => g.UserId == userId))
            return;

        throw new ForbiddenException("You do not have access to this student profile.");
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
