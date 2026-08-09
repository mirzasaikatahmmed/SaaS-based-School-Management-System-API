using System.Globalization;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.Parents;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Helpers;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class ParentService : IParentService
{
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    private const long MaxImageBytes = 2 * 1024 * 1024;

    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ITenantSchemaProvisioner _schemaProvisioner;
    private readonly IStorageService _storageService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPasswordRevealService _passwordReveal;

    public ParentService(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ITenantSchemaProvisioner schemaProvisioner,
        IStorageService storageService,
        IHttpContextAccessor httpContextAccessor,
        IPasswordRevealService passwordReveal)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _schemaProvisioner = schemaProvisioner;
        _storageService = storageService;
        _httpContextAccessor = httpContextAccessor;
        _passwordReveal = passwordReveal;
    }

    public async Task<ParentListResponseDto> GetListAsync(
        ParentListFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        EnsureTenant();
        EnsureCanReadList();
        await EnsureReadyAsync(cancellationToken);

        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize is < 1 or > 200 ? 20 : filter.PageSize;
        var branch = _tenantContext.TenantName ?? string.Empty;

        var (items, total) = await _unitOfWork.Guardians.SearchAsync(new GuardianSearchFilter
        {
            Search = filter.Search,
            IsActive = true,
            SortBy = filter.SortBy,
            SortDir = filter.SortDir,
            Page = page,
            PageSize = pageSize
        }, cancellationToken);

        var data = items.Select((g, i) => new ParentListItemDto
        {
            Id = g.Id,
            Sl = (page - 1) * pageSize + i + 1,
            Branch = branch,
            GuardianName = g.Name,
            Occupation = g.Occupation,
            ReferenceNo = g.ReferenceNo,
            Email = g.Email,
            IsLoginActive = g.User?.Active ?? g.IsLoginActive
        }).ToList();

        return new ParentListResponseDto
        {
            Data = data,
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize)
        };
    }

    public async Task<ParentDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EnsureTenant();
        await EnsureReadyAsync(cancellationToken);

        var guardian = await _unitOfWork.Guardians.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Parent '{id}' not found.");

        EnsureCanReadParent(guardian);
        return await MapDetailAsync(guardian, cancellationToken);
    }

    public async Task<ParentDetailDto> GetMeAsync(CancellationToken cancellationToken = default)
    {
        EnsureTenant();
        await EnsureReadyAsync(cancellationToken);

        var roles = GetCurrentRoles();
        if (!roles.Contains(AppConstants.Roles.Parent))
            throw new ForbiddenException("Only guardians can access this endpoint.");

        var userId = GetCurrentUserId();
        var guardian = await _unitOfWork.Guardians.GetPrimaryByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Parent profile not found for the current user.");

        return await MapDetailAsync(guardian, cancellationToken);
    }

    public async Task<ParentDetailDto> CreateAsync(AddParentDto dto, CancellationToken cancellationToken = default)
    {
        EnsureTenant();
        EnsureCanManage();
        await EnsureReadyAsync(cancellationToken);

        if (!string.Equals(dto.Password, dto.RetypePassword, StringComparison.Ordinal))
            throw new AppException("Password and Retype Password must match.", 400);

        var username = dto.Username.Trim().ToLowerInvariant();
        if (await _unitOfWork.Users.UsernameExistsAsync(username, cancellationToken))
            throw new ConflictException($"Username '{username}' already exists.");

        var email = string.IsNullOrWhiteSpace(dto.Email)
            ? $"{username}@guardians.local"
            : dto.Email.Trim().ToLowerInvariant();

        if (await _unitOfWork.Users.EmailExistsAsync(email, cancellationToken))
            throw new ConflictException($"Email '{email}' already exists.");

        var studentIds = (dto.StudentIds ?? []).Distinct().ToList();
        foreach (var studentId in studentIds)
        {
            _ = await _unitOfWork.Students.GetByIdAsync(studentId, cancellationToken)
                ?? throw new NotFoundException($"Student '{studentId}' not found.");
        }

        var parentRole = await _unitOfWork.Users.GetRoleByNameAsync(AppConstants.Roles.Parent, cancellationToken)
            ?? throw new AppException("Parent/Guardian role is not seeded in this tenant.", 500);

        await _unitOfWork.BeginTenantTransactionAsync(cancellationToken);
        try
        {
            var nameParts = dto.Name.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                Username = username,
                FirstName = nameParts[0],
                LastName = nameParts.Length > 1 ? nameParts[1] : string.Empty,
                Mobileno = dto.MobileNo.Trim(),
                Active = true,
                IsEmailVerified = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _passwordReveal.Apply(user, dto.Password);
            await _unitOfWork.Users.AddAsync(user, cancellationToken);
            await _unitOfWork.Users.AddUserRoleAsync(new UserRole
            {
                UserId = user.Id,
                RoleId = parentRole.Id
            }, cancellationToken);

            Guardian primary;
            if (studentIds.Count == 0)
            {
                primary = await CreateGuardianRowAsync(dto, user.Id, null, cancellationToken);
            }
            else
            {
                primary = await CreateGuardianRowAsync(dto, user.Id, studentIds[0], cancellationToken);
                for (var i = 1; i < studentIds.Count; i++)
                    await CreateGuardianRowAsync(dto, user.Id, studentIds[i], cancellationToken);
            }

            await _unitOfWork.SaveTenantChangesAsync(cancellationToken);
            await _unitOfWork.CommitTenantTransactionAsync(cancellationToken);

            var created = await _unitOfWork.Guardians.GetByIdWithDetailsAsync(primary.Id, cancellationToken) ?? primary;
            return await MapDetailAsync(created, cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTenantTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<ParentDetailDto> UpdateAsync(
        Guid id,
        UpdateParentDto dto,
        CancellationToken cancellationToken = default)
    {
        EnsureTenant();
        EnsureCanManage();
        await EnsureReadyAsync(cancellationToken);

        var guardian = await _unitOfWork.Guardians.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Parent '{id}' not found.");

        if (!string.IsNullOrWhiteSpace(dto.Name)) guardian.Name = dto.Name.Trim();
        else throw new AppException("Name is required.", 400);

        if (dto.Relation is not null)
            guardian.Relation = string.IsNullOrWhiteSpace(dto.Relation) ? "Guardian" : dto.Relation.Trim();
        if (dto.FatherName is not null) guardian.FatherName = dto.FatherName;
        if (dto.MotherName is not null) guardian.MotherName = dto.MotherName;
        if (dto.Occupation is not null) guardian.Occupation = dto.Occupation;
        if (dto.Income.HasValue) guardian.Income = dto.Income;
        if (dto.Education is not null) guardian.Education = dto.Education;
        if (dto.City is not null) guardian.City = dto.City;
        if (dto.State is not null) guardian.State = dto.State;
        if (!string.IsNullOrWhiteSpace(dto.MobileNo)) guardian.MobileNo = dto.MobileNo.Trim();
        if (dto.Email is not null) guardian.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim().ToLowerInvariant();
        if (dto.Address is not null) guardian.Address = dto.Address;
        if (dto.AlternativeParentName is not null) guardian.AlternativeParentName = dto.AlternativeParentName;
        if (dto.AlternativeParentRelation is not null) guardian.AlternativeParentRelation = dto.AlternativeParentRelation;
        if (dto.AlternativeParentMobileNo is not null) guardian.AlternativeParentMobileNo = dto.AlternativeParentMobileNo;
        if (dto.FacebookUrl is not null) guardian.FacebookUrl = dto.FacebookUrl;
        if (dto.TwitterUrl is not null) guardian.TwitterUrl = dto.TwitterUrl;
        if (dto.LinkedInUrl is not null) guardian.LinkedInUrl = dto.LinkedInUrl;
        guardian.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Guardians.UpdateAsync(guardian, cancellationToken);

        // Student linking removed from UpdateParentDto — use create/link via Add if needed
        await _unitOfWork.SaveTenantChangesAsync(cancellationToken);
        var updated = await _unitOfWork.Guardians.GetByIdWithDetailsAsync(id, cancellationToken) ?? guardian;
        return await MapDetailAsync(updated, cancellationToken);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EnsureTenant();
        EnsureCanManage();
        await EnsureReadyAsync(cancellationToken);

        var guardian = await _unitOfWork.Guardians.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Parent '{id}' not found.");

        var linked = guardian.UserId.HasValue
            ? await _unitOfWork.Guardians.GetByUserIdAsync(guardian.UserId.Value, cancellationToken)
            : [guardian];

        var soleCount = 0;
        foreach (var link in linked.Where(g => g.StudentId.HasValue && g.IsActive))
        {
            var others = await _unitOfWork.Guardians.CountActiveGuardiansForStudentAsync(
                link.StudentId!.Value, link.Id, cancellationToken);
            if (others == 0)
                soleCount++;
        }

        if (soleCount > 0)
            throw new AppException(
                $"Guardian is the sole guardian for {soleCount} student(s). Assign another guardian first.",
                400);

        foreach (var link in linked.Where(g => g.IsActive))
        {
            link.IsActive = false;
            link.IsLoginActive = false;
            link.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Guardians.UpdateAsync(link, cancellationToken);
        }

        if (guardian.UserId.HasValue)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(guardian.UserId.Value, cancellationToken);
            if (user is not null)
            {
                user.Active = false;
                user.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
            }
        }

        await _unitOfWork.SaveTenantChangesAsync(cancellationToken);
    }

    public async Task<ParentDetailDto> UploadPhotoAsync(
        Guid id,
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        EnsureTenant();
        EnsureCanManage();
        await EnsureReadyAsync(cancellationToken);
        ValidateImage(fileName, contentType, fileStream);

        var guardian = await _unitOfWork.Guardians.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Parent '{id}' not found.");

        var slug = RequireTenantSlug();
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        var objectKey = $"{AppConstants.StorageFolders.Guardians}/{guardian.Id}/profile{ext}";

        if (!string.IsNullOrWhiteSpace(guardian.ProfilePictureUrl))
        {
            try { await _storageService.DeleteFileAsync(slug, guardian.ProfilePictureUrl, cancellationToken); }
            catch { /* ignore */ }
        }

        await _storageService.UploadObjectAsync(slug, objectKey, fileStream, contentType, cancellationToken);
        guardian.ProfilePictureUrl = objectKey;
        guardian.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Guardians.UpdateAsync(guardian, cancellationToken);
        await _unitOfWork.SaveTenantChangesAsync(cancellationToken);

        return await MapDetailAsync(guardian, cancellationToken);
    }

    public async Task<(byte[] Content, string ContentType, string FileName)> ExportAsync(
        ParentListFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        EnsureTenant();
        EnsureCanManage();
        await EnsureReadyAsync(cancellationToken);

        var branch = _tenantContext.TenantName ?? string.Empty;
        var (items, _) = await _unitOfWork.Guardians.SearchAsync(new GuardianSearchFilter
        {
            Search = filter.Search,
            IsActive = true,
            SortBy = filter.SortBy,
            SortDir = filter.SortDir,
            Page = 1,
            PageSize = 10_000
        }, cancellationToken);

        var rows = items.Select((g, i) => new ParentListItemDto
        {
            Id = g.Id,
            Sl = i + 1,
            Branch = branch,
            GuardianName = g.Name,
            Occupation = g.Occupation,
            ReferenceNo = g.ReferenceNo,
            Email = g.Email,
            IsLoginActive = g.User?.Active ?? g.IsLoginActive
        }).ToList();

        var fmt = (filter.Export ?? "csv").Trim().ToLowerInvariant();
        return fmt switch
        {
            "csv" or "excel" => BuildCsvExport(rows, fmt == "excel"),
            "pdf" => BuildPdfExport(rows),
            _ => throw new AppException("Unsupported export format. Use csv, excel, or pdf.", 400)
        };
    }

    private async Task<Guardian> CreateGuardianRowAsync(
        AddParentDto dto,
        Guid userId,
        Guid? studentId,
        CancellationToken cancellationToken)
    {
        var guardian = new Guardian
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            UserId = userId,
            Name = dto.Name.Trim(),
            Relation = dto.Relation.Trim(),
            FatherName = dto.FatherName,
            MotherName = dto.MotherName,
            Occupation = dto.Occupation.Trim(),
            Income = dto.Income,
            Education = dto.Education,
            City = dto.City,
            State = dto.State,
            MobileNo = dto.MobileNo.Trim(),
            Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim().ToLowerInvariant(),
            Address = dto.Address,
            AlternativeParentName = dto.AlternativeParentName,
            AlternativeParentRelation = dto.AlternativeParentRelation,
            AlternativeParentMobileNo = dto.AlternativeParentMobileNo,
            FacebookUrl = dto.FacebookUrl,
            TwitterUrl = dto.TwitterUrl,
            LinkedInUrl = dto.LinkedInUrl,
            IsPrimary = true,
            IsActive = true,
            IsLoginActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _unitOfWork.Guardians.AddAsync(guardian, cancellationToken);
        return guardian;
    }

    private async Task<ParentDetailDto> MapDetailAsync(Guardian g, CancellationToken cancellationToken)
    {
        var wards = new List<LinkedStudentDto>();
        IReadOnlyList<Guardian> links = g.UserId.HasValue
            ? await _unitOfWork.Guardians.GetByUserIdAsync(g.UserId.Value, cancellationToken)
            : g.StudentId.HasValue ? [g] : Array.Empty<Guardian>();

        foreach (var link in links.Where(x => x.Student is not null || x.StudentId.HasValue))
        {
            var student = link.Student;
            if (student is null && link.StudentId.HasValue)
            {
                student = await _unitOfWork.Students.GetByIdWithDetailsAsync(link.StudentId.Value, cancellationToken);
            }

            if (student is null) continue;

            wards.Add(new LinkedStudentDto
            {
                StudentId = student.Id,
                Name = string.IsNullOrWhiteSpace(student.LastName)
                    ? student.FirstName
                    : $"{student.FirstName} {student.LastName}",
                RegisterNo = student.RegisterNo,
                ClassName = student.Class?.Name ?? string.Empty,
                SectionName = student.Section?.Name ?? string.Empty,
                PhotoUrl = await PresignAsync(student.ProfilePictureUrl, cancellationToken)
            });
        }

        return new ParentDetailDto
        {
            Id = g.Id,
            ReferenceNo = g.ReferenceNo,
            Name = g.Name,
            Relation = g.Relation,
            FatherName = g.FatherName,
            MotherName = g.MotherName,
            Occupation = g.Occupation,
            Income = g.Income,
            Education = g.Education,
            City = g.City,
            State = g.State,
            MobileNo = g.MobileNo,
            Email = g.Email,
            Address = g.Address,
            PhotoUrl = await PresignAsync(g.ProfilePictureUrl, cancellationToken),
            Username = g.User?.Username,
            IsLoginActive = g.User?.Active ?? g.IsLoginActive,
            AlternativeParentName = g.AlternativeParentName,
            AlternativeParentRelation = g.AlternativeParentRelation,
            AlternativeParentMobileNo = g.AlternativeParentMobileNo,
            FacebookUrl = g.FacebookUrl,
            TwitterUrl = g.TwitterUrl,
            LinkedInUrl = g.LinkedInUrl,
            Students = wards,
            CreatedAt = g.CreatedAt
        };
    }

    private async Task<string?> PresignAsync(string? objectKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(objectKey)) return null;
        var slug = _tenantContext.TenantSlug;
        if (string.IsNullOrWhiteSpace(slug)) return objectKey;
        try { return await _storageService.GetPresignedUrlAsync(slug, objectKey, cancellationToken); }
        catch { return objectKey; }
    }

    private static (byte[] Content, string ContentType, string FileName) BuildCsvExport(
        List<ParentListItemDto> items, bool asExcel)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Sl,Branch,GuardianName,Occupation,ReferenceNo,Email,IsLoginActive");
        foreach (var p in items)
        {
            sb.AppendLine(string.Join(',',
                p.Sl,
                Csv(p.Branch),
                Csv(p.GuardianName),
                Csv(p.Occupation),
                Csv(p.ReferenceNo),
                Csv(p.Email),
                p.IsLoginActive ? "Yes" : "No"));
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return asExcel
            ? (bytes, "application/vnd.ms-excel", $"parents-{DateTime.UtcNow:yyyyMMdd}.xls")
            : (bytes, "text/csv", $"parents-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    private static string Csv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return $"\"{value.Replace("\"", "\"\"")}\"";
    }

    private (byte[] Content, string ContentType, string FileName) BuildPdfExport(List<ParentListItemDto> items)
    {
        var school = _tenantContext.TenantName ?? "School";
        var lines = new List<string> { school, "Parents Export", "" };
        foreach (var p in items)
            lines.Add($"{p.Sl}. {p.ReferenceNo} | {p.GuardianName} | {p.Occupation}");

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

        return (Encoding.ASCII.GetBytes(pdf), "application/pdf", $"parents-{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    private static void ValidateImage(string fileName, string contentType, Stream stream)
    {
        var ext = Path.GetExtension(fileName);
        if (!AllowedImageExtensions.Contains(ext))
            throw new AppException("Only jpg, jpeg, png, and webp images are allowed.", 400);
        if (!contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            throw new AppException("Invalid image content type.", 400);
        if (stream.CanSeek && stream.Length > MaxImageBytes)
            throw new AppException("Image must be 2MB or smaller.", 400);
    }

    private string RequireTenantSlug() =>
        _tenantContext.TenantSlug ?? throw new AppException("Tenant slug is not resolved.", 400);

    private async Task EnsureReadyAsync(CancellationToken cancellationToken)
    {
        await _schemaProvisioner.EnsureAdmissionModuleAsync(_tenantContext.SchemaName!, cancellationToken);
        await _schemaProvisioner.EnsureGuardianParentFieldsAsync(_tenantContext.SchemaName!, cancellationToken);
        await _schemaProvisioner.EnsureGuardianSocialAndAlternativeFieldsAsync(_tenantContext.SchemaName!, cancellationToken);
        await _unitOfWork.Guardians.BackfillMissingReferenceNosAsync(cancellationToken);
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
        throw new ForbiddenException("Only Super Admin or School Admin can manage parents.");
    }

    private void EnsureCanReadList()
    {
        var roles = GetCurrentRoles();
        if (roles.Contains(AppConstants.Roles.SuperAdmin) ||
            roles.Contains(AppConstants.Roles.Admin) ||
            roles.Contains(AppConstants.Roles.Teacher))
            return;
        throw new ForbiddenException("You do not have access to the parents list.");
    }

    private void EnsureCanReadParent(Guardian guardian)
    {
        var roles = GetCurrentRoles();
        if (roles.Contains(AppConstants.Roles.SuperAdmin) ||
            roles.Contains(AppConstants.Roles.Admin) ||
            roles.Contains(AppConstants.Roles.Teacher))
            return;

        var userId = GetCurrentUserId();
        if (roles.Contains(AppConstants.Roles.Parent) && guardian.UserId == userId)
            return;

        throw new ForbiddenException("You do not have access to this parent profile.");
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
