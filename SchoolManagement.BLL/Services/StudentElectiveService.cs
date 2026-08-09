using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.Academic;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class StudentElectiveService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http) : IStudentElectiveService
{
    public async Task<StudentElectiveListDto> GetClassElectivesAsync(
        Guid classId, Guid sectionId, int academicYear, string electiveGroup = "4th",
        CancellationToken ct = default)
    {
        await Ready(ct);
        Read();

        var group = string.IsNullOrWhiteSpace(electiveGroup) ? ElectiveGroups.Fourth : electiveGroup.Trim();
        var assignment = await uow.ClassSubjectAssignments.GetByClassSectionAsync(classId, sectionId, ct);
        var options = assignment?.Items
            .Where(i => i.IsElective && string.Equals(i.ElectiveGroup, group, StringComparison.OrdinalIgnoreCase))
            .Select(i => new AssignedSubjectDto
            {
                Id = i.Subject.Id,
                Name = i.Subject.Name,
                Code = i.Subject.Code,
                IsElective = true,
                ElectiveGroup = i.ElectiveGroup
            }).ToList() ?? [];

        // Biology (CanBeAdditional) + elective options (Higher Math / Agriculture) for Additional declaration UI
        var biologyPool = (await uow.Subjects.GetAllAsync(ct))
            .Where(s => s.IsActive && s.CanBeAdditional)
            .Select(s => new AssignedSubjectDto
            {
                Id = s.Id,
                Name = s.Name,
                Code = s.Code,
                IsElective = false,
                ElectiveGroup = null
            });
        var additionalPool = options.Concat(biologyPool)
            .GroupBy(x => x.Id)
            .Select(g => g.First())
            .ToList();

        var students = await uow.MarkEntries.GetActiveStudentsByClassSectionAsync(classId, sectionId, ct);
        var enrollments = (await uow.StudentSubjectEnrollments.GetForClassAsync(classId, sectionId, academicYear, group, ct))
            .ToDictionary(e => e.StudentId);

        var rows = students.Select(s =>
        {
            enrollments.TryGetValue(s.Id, out var en);
            return new StudentElectiveRowDto
            {
                StudentId = s.Id,
                StudentName = string.IsNullOrWhiteSpace(s.LastName) ? s.FirstName.Trim() : $"{s.FirstName.Trim()} {s.LastName.Trim()}",
                RegisterNo = s.RegisterNo,
                Roll = s.Roll,
                SubjectId = en?.SubjectId,
                SubjectName = en?.Subject?.Name,
                SubjectCode = en?.Subject?.Code,
                AdditionalSubjectId = en?.AdditionalSubjectId,
                AdditionalSubjectName = en?.AdditionalSubject?.Name,
                ElectiveGroup = group,
                IsAssigned = en is not null
            };
        }).ToList();

        return new StudentElectiveListDto
        {
            ClassId = classId,
            SectionId = sectionId,
            AcademicYear = academicYear,
            ElectiveGroup = group,
            Options = options,
            AdditionalOptions = additionalPool,
            Students = rows
        };
    }

    public async Task<StudentElectiveRowDto> AssignAsync(AssignStudentElectiveDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();

        var group = string.IsNullOrWhiteSpace(dto.ElectiveGroup) ? ElectiveGroups.Fourth : dto.ElectiveGroup.Trim();
        await ValidateElectiveAsync(dto.ClassId, dto.SectionId, dto.SubjectId, group, ct);
        var additionalId = await ResolveAdditionalAsync(dto.SubjectId, dto.AdditionalSubjectId, ct);

        var student = await uow.Students.GetByIdAsync(dto.StudentId, ct)
            ?? throw new NotFoundException("Student not found.");
        if (student.ClassId != dto.ClassId || student.SectionId != dto.SectionId)
            throw new AppException("Student is not in the selected class/section.", 400);

        await uow.StudentSubjectEnrollments.UpsertAsync(new StudentSubjectEnrollment
        {
            Id = Guid.NewGuid(),
            StudentId = dto.StudentId,
            SubjectId = dto.SubjectId,
            AdditionalSubjectId = additionalId,
            ClassId = dto.ClassId,
            SectionId = dto.SectionId,
            AcademicYear = dto.AcademicYear,
            ElectiveGroup = group,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }, ct);
        await uow.SaveTenantChangesAsync(ct);

        var list = await GetClassElectivesAsync(dto.ClassId, dto.SectionId, dto.AcademicYear, group, ct);
        return list.Students.First(s => s.StudentId == dto.StudentId);
    }

    public async Task<StudentElectiveListDto> BulkAssignAsync(BulkAssignStudentElectiveDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();

        var group = string.IsNullOrWhiteSpace(dto.ElectiveGroup) ? ElectiveGroups.Fourth : dto.ElectiveGroup.Trim();
        if (dto.Choices.Count == 0)
            throw new AppException("At least one choice is required.", 400);

        var optionIds = await GetElectiveOptionIdsAsync(dto.ClassId, dto.SectionId, group, ct);
        var entries = new List<StudentSubjectEnrollment>();
        foreach (var choice in dto.Choices)
        {
            if (!optionIds.Contains(choice.SubjectId))
                throw new AppException($"Subject '{choice.SubjectId}' is not Higher Math/Agriculture elective for group '{group}'.", 400);
            var additionalId = await ResolveAdditionalAsync(choice.SubjectId, choice.AdditionalSubjectId, ct);

            entries.Add(new StudentSubjectEnrollment
            {
                Id = Guid.NewGuid(),
                StudentId = choice.StudentId,
                SubjectId = choice.SubjectId,
                AdditionalSubjectId = additionalId,
                ClassId = dto.ClassId,
                SectionId = dto.SectionId,
                AcademicYear = dto.AcademicYear,
                ElectiveGroup = group,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await uow.BeginTenantTransactionAsync(ct);
        try
        {
            await uow.StudentSubjectEnrollments.UpsertRangeAsync(entries, ct);
            await uow.SaveTenantChangesAsync(ct);
            await uow.CommitTenantTransactionAsync(ct);
        }
        catch
        {
            await uow.RollbackTenantTransactionAsync(ct);
            throw;
        }

        return await GetClassElectivesAsync(dto.ClassId, dto.SectionId, dto.AcademicYear, group, ct);
    }

    private async Task ValidateElectiveAsync(Guid classId, Guid sectionId, Guid subjectId, string group, CancellationToken ct)
    {
        var optionIds = await GetElectiveOptionIdsAsync(classId, sectionId, group, ct);
        if (optionIds.Count == 0)
            throw new AppException(
                $"No elective subjects for group '{group}'. Mark Higher Math and Agriculture as elective (not Biology).",
                400);
        if (!optionIds.Contains(subjectId))
            throw new AppException("Elective must be Higher Math or Agriculture (Biology is not an elective).", 400);
    }

    /// <summary>Additional = elective OR CanBeAdditional (Biology). Default Biology if available.</summary>
    private async Task<Guid> ResolveAdditionalAsync(Guid electiveSubjectId, Guid? requested, CancellationToken ct)
    {
        var biology = (await uow.Subjects.GetAllAsync(ct)).FirstOrDefault(s => s.IsActive && s.CanBeAdditional);

        if (!requested.HasValue)
        {
            if (biology is not null) return biology.Id;
            return electiveSubjectId;
        }

        if (requested.Value == electiveSubjectId) return requested.Value;
        if (biology is not null && requested.Value == biology.Id) return requested.Value;

        var sub = await uow.Subjects.GetByIdAsync(requested.Value, ct)
            ?? throw new NotFoundException("Additional subject not found.");
        if (sub.CanBeAdditional) return sub.Id;

        throw new AppException(
            "Additional subject must be either your elective (Higher Math/Agriculture) or Biology (CanBeAdditional).",
            400);
    }

    private async Task<HashSet<Guid>> GetElectiveOptionIdsAsync(Guid classId, Guid sectionId, string group, CancellationToken ct)
    {
        var assignment = await uow.ClassSubjectAssignments.GetByClassSectionAsync(classId, sectionId, ct)
            ?? throw new AppException("Configure class–subject assignment first.", 400);
        return assignment.Items
            .Where(i => i.IsElective && string.Equals(i.ElectiveGroup, group, StringComparison.OrdinalIgnoreCase))
            .Select(i => i.SubjectId)
            .ToHashSet();
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
            throw new ForbiddenException("Only Super Admin or School Admin can assign student electives.");
    }

    private void Read()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin)
            && !r.Contains(AppConstants.Roles.Teacher))
            throw new ForbiddenException("You do not have access to student electives.");
    }
}
