using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.Reports;
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

public class StudentReportService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IPasswordRevealService passwordReveal,
    IStorageService storage,
    IHttpContextAccessor http) : IStudentReportService
{
    public async Task<LoginCredentialReportDto> GetLoginCredentialsAsync(
        StudentReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        Manage();

        var page = filter.Page < 1 ? 1 : filter.Page;
        var size = string.IsNullOrWhiteSpace(filter.Export) ? (filter.PageSize is < 1 or > 200 ? 50 : filter.PageSize) : 10_000;

        var (items, total) = await uow.Students.SearchAsync(new StudentSearchFilter
        {
            ClassId = filter.ClassId,
            SectionId = filter.SectionId,
            Search = filter.Search,
            IsActive = true,
            Page = page,
            PageSize = size,
            SortBy = "registerNo",
            SortDir = "asc"
        }, cancellationToken);

        var rows = new List<LoginCredentialRowDto>();
        var i = (page - 1) * size;
        foreach (var s in items)
        {
            i++;
            var primary = s.Guardians.FirstOrDefault(g => g.IsPrimary && g.IsActive)
                          ?? s.Guardians.FirstOrDefault(g => g.IsActive);
            var parentUser = primary?.User;
            var studentPassword = passwordReveal.Unprotect(s.User?.PasswordRevealEncrypted);
            var parentPassword = passwordReveal.Unprotect(parentUser?.PasswordRevealEncrypted);

            string? photo = null;
            if (!string.IsNullOrWhiteSpace(s.ProfilePictureUrl) && !string.IsNullOrEmpty(tenant.TenantSlug))
            {
                try { photo = await storage.GetPresignedUrlAsync(tenant.TenantSlug!, s.ProfilePictureUrl, cancellationToken); }
                catch { photo = s.ProfilePictureUrl; }
            }

            rows.Add(new LoginCredentialRowDto
            {
                StudentId = s.Id,
                Sl = i,
                PhotoUrl = photo,
                Name = StudentName(s),
                ClassName = s.Class?.Name ?? string.Empty,
                SectionName = s.Section?.Name ?? string.Empty,
                RegisterNo = s.RegisterNo,
                Roll = s.Roll,
                GuardianName = primary?.Name,
                StudentUsername = s.User?.Username ?? string.Empty,
                StudentPassword = studentPassword,
                ParentUsername = parentUser?.Username,
                ParentPassword = parentPassword,
                PasswordRevealAvailable = studentPassword is not null || parentPassword is not null
            });
        }

        return new LoginCredentialReportDto
        {
            Data = rows,
            TotalCount = total,
            Page = page,
            PageSize = size,
            TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)size)
        };
    }

    public async Task<ResetStudentPasswordResultDto> ResetPasswordAsync(
        Guid studentId, ResetStudentPasswordDto dto, CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        Manage();

        var student = await uow.Students.GetByIdWithDetailsAsync(studentId, cancellationToken)
            ?? throw new NotFoundException($"Student '{studentId}' not found.");

        var studentUser = student.User
            ?? await uow.Users.GetByIdAsync(student.UserId, cancellationToken)
            ?? throw new NotFoundException("Student login account not found.");

        var studentPassword = string.IsNullOrWhiteSpace(dto.NewPassword)
            ? GenerateTempPassword()
            : dto.NewPassword.Trim();
        if (studentPassword.Length < 4)
            throw new AppException("Student password must be at least 4 characters.", 400);

        passwordReveal.Apply(studentUser, studentPassword);
        await uow.Users.UpdateAsync(studentUser, cancellationToken);

        string? parentUsername = null;
        string? parentPassword = null;

        if (dto.ResetParentPassword)
        {
            var primary = student.Guardians.FirstOrDefault(g => g.IsPrimary && g.IsActive && g.UserId.HasValue)
                          ?? student.Guardians.FirstOrDefault(g => g.IsActive && g.UserId.HasValue);
            if (primary?.UserId is Guid parentUserId)
            {
                var parentUser = primary.User
                    ?? await uow.Users.GetByIdAsync(parentUserId, cancellationToken);
                if (parentUser is not null)
                {
                    parentPassword = string.IsNullOrWhiteSpace(dto.NewParentPassword)
                        ? GenerateTempPassword()
                        : dto.NewParentPassword.Trim();
                    if (parentPassword.Length < 4)
                        throw new AppException("Parent password must be at least 4 characters.", 400);
                    passwordReveal.Apply(parentUser, parentPassword);
                    await uow.Users.UpdateAsync(parentUser, cancellationToken);
                    parentUsername = parentUser.Username;
                }
            }
        }

        await uow.SaveTenantChangesAsync(cancellationToken);

        return new ResetStudentPasswordResultDto
        {
            StudentId = student.Id,
            StudentUsername = studentUser.Username,
            StudentPassword = studentPassword,
            ParentUsername = parentUsername,
            ParentPassword = parentPassword
        };
    }

    public async Task<AdmissionReportDto> GetAdmissionReportAsync(
        StudentReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        Manage();

        var page = filter.Page < 1 ? 1 : filter.Page;
        var size = string.IsNullOrWhiteSpace(filter.Export) ? (filter.PageSize is < 1 or > 200 ? 50 : filter.PageSize) : 10_000;
        var from = filter.FromDate?.Date ?? new DateTime(DateTime.UtcNow.Year, 1, 1);
        var to = filter.ToDate?.Date ?? new DateTime(DateTime.UtcNow.Year, 12, 31);

        // Search then filter admission date in memory for range (search API has no date filter)
        var (all, _) = await uow.Students.SearchAsync(new StudentSearchFilter
        {
            ClassId = filter.ClassId,
            SectionId = filter.SectionId,
            Search = filter.Search,
            IsActive = true,
            Page = 1,
            PageSize = 10_000,
            SortBy = "registerNo",
            SortDir = "asc"
        }, cancellationToken);

        var matched = all
            .Where(s => s.AdmissionDate.Date >= from && s.AdmissionDate.Date <= to)
            .OrderBy(s => s.AdmissionDate)
            .ThenBy(s => s.RegisterNo)
            .ToList();

        var total = matched.Count;
        var pageItems = matched.Skip((page - 1) * size).Take(size).ToList();
        var rows = pageItems.Select((s, idx) =>
        {
            var primary = s.Guardians.FirstOrDefault(g => g.IsPrimary && g.IsActive)
                          ?? s.Guardians.FirstOrDefault(g => g.IsActive);
            return new AdmissionReportRowDto
            {
                StudentId = s.Id,
                Sl = (page - 1) * size + idx + 1,
                Name = StudentName(s),
                Gender = s.Gender,
                RegisterNo = s.RegisterNo,
                Roll = s.Roll,
                ClassName = s.Class?.Name ?? string.Empty,
                SectionName = s.Section?.Name ?? string.Empty,
                GuardianName = primary?.Name,
                AdmissionDate = s.AdmissionDate
            };
        }).ToList();

        return new AdmissionReportDto
        {
            Summary = $"Total of {total} students Admission during this period from {from:dd/MMM/yyyy} to {to:dd/MMM/yyyy}",
            Data = rows,
            TotalCount = total,
            Page = page,
            PageSize = size,
            TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)size)
        };
    }

    public async Task<IReadOnlyList<ClassSectionReportRowDto>> GetClassSectionReportAsync(
        CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        Manage();

        var classes = await uow.ClassControls.GetAllWithSectionsAsync(cancellationToken);
        var (students, _) = await uow.Students.SearchAsync(new StudentSearchFilter
        {
            IsActive = true,
            Page = 1,
            PageSize = 50_000
        }, cancellationToken);

        var byClass = students
            .Where(s => s.ClassId.HasValue)
            .GroupBy(s => s.ClassId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var rows = new List<ClassSectionReportRowDto>();
        var sl = 0;
        foreach (var cls in classes.OrderBy(c => c.Name))
        {
            sl++;
            byClass.TryGetValue(cls.Id, out var classStudents);
            classStudents ??= [];

            var sections = classStudents
                .Where(s => s.SectionId.HasValue)
                .GroupBy(s => new { s.SectionId, Name = s.Section?.Name ?? "?" })
                .Select(g => new ClassSectionCountDto
                {
                    SectionId = g.Key.SectionId!.Value,
                    SectionName = g.Key.Name,
                    StudentCount = g.Count()
                })
                .OrderBy(x => x.SectionName)
                .ToList();

            rows.Add(new ClassSectionReportRowDto
            {
                Sl = sl,
                ClassId = cls.Id,
                ClassName = cls.Name,
                Sections = sections,
                TotalStudents = classStudents.Count
            });
        }

        return rows;
    }

    public async Task<IReadOnlyList<SiblingReportRowDto>> GetSiblingReportAsync(
        StudentReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        Manage();

        var (students, _) = await uow.Students.SearchAsync(new StudentSearchFilter
        {
            ClassId = filter.ClassId,
            SectionId = filter.SectionId,
            Search = filter.Search,
            IsActive = true,
            Page = 1,
            PageSize = 50_000
        }, cancellationToken);

        // Group siblings by shared guardian login (UserId) or shared mobile number.
        var groups = new Dictionary<string, (Guardian Guardian, List<Student> Students)>(StringComparer.OrdinalIgnoreCase);

        foreach (var s in students)
        {
            var primary = s.Guardians.FirstOrDefault(g => g.IsPrimary && g.IsActive)
                          ?? s.Guardians.FirstOrDefault(g => g.IsActive);
            if (primary is null) continue;

            var key = primary.UserId.HasValue
                ? $"user:{primary.UserId}"
                : $"mobile:{(primary.MobileNo ?? string.Empty).Trim()}|{primary.Name.Trim()}";

            if (!groups.TryGetValue(key, out var bucket))
            {
                bucket = (primary, []);
                groups[key] = bucket;
            }
            bucket.Students.Add(s);
        }

        var siblingGroups = groups.Values
            .Where(g => g.Students.Count >= 2)
            .OrderBy(g => g.Guardian.Name)
            .ToList();

        return siblingGroups.Select((g, idx) => new SiblingReportRowDto
        {
            Sl = idx + 1,
            GuardianName = g.Guardian.Name,
            MobileNo = g.Guardian.MobileNo,
            FatherName = g.Guardian.FatherName,
            MotherName = g.Guardian.MotherName,
            Occupation = g.Guardian.Occupation,
            Siblings = g.Students
                .OrderBy(s => s.Class?.Name)
                .ThenBy(s => s.RegisterNo)
                .Select(s => new SiblingStudentDto
                {
                    StudentId = s.Id,
                    Name = StudentName(s),
                    RegisterNo = s.RegisterNo,
                    ClassName = $"{s.Class?.Name} {s.Section?.Name}".Trim(),
                    Gender = s.Gender
                }).ToList()
        }).ToList();
    }

    private static string StudentName(Student s)
        => string.IsNullOrWhiteSpace(s.LastName) ? s.FirstName.Trim() : $"{s.FirstName.Trim()} {s.LastName.Trim()}";

    private static string GenerateTempPassword()
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
        var bytes = RandomNumberGenerator.GetBytes(8);
        var sb = new StringBuilder(8);
        foreach (var b in bytes)
            sb.Append(alphabet[b % alphabet.Length]);
        return sb.ToString();
    }

    private async Task Ready(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(tenant.SchemaName))
            throw new AppException("X-Tenant-ID header is required.", 400);
        await provisioner.EnsureSettingsModuleAsync(tenant.SchemaName!, ct);
    }

    private HashSet<string> Roles() =>
        http.HttpContext?.User.FindAll("role").Concat(http.HttpContext.User.FindAll(ClaimTypes.Role))
            .Select(x => x.Value).ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];

    private void Manage()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin))
            throw new ForbiddenException("Only Super Admin or School Admin can access student reports.");
    }
}
