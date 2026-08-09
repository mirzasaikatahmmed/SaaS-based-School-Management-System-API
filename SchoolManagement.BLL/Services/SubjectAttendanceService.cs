using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.Attendance;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class SubjectAttendanceService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http) : ISubjectAttendanceService
{
    public async Task<SubjectAttendanceForDateResponseDto> GetForDateAsync(
        Guid classId, Guid sectionId, Guid subjectId, DateTime date, CancellationToken ct = default)
    {
        await Ready(ct);
        ManageOrTeacher();

        var students = await uow.StudentAttendances.GetActiveStudentsAsync(classId, sectionId, ct);
        var academicYear = students.FirstOrDefault()?.AcademicYear ?? date.Year;
        students = (await Helpers.ElectiveSubjectHelper.FilterStudentsForSubjectAsync(
            uow, classId, sectionId, subjectId, academicYear, students, ct)).ToList();

        var existing = await uow.StudentSubjectAttendances.GetForDateAsync(classId, sectionId, subjectId, date, ct);
        var byStudent = existing.ToDictionary(a => a.StudentId);
        var subject = await uow.Subjects.GetByIdAsync(subjectId, ct)
            ?? throw new NotFoundException("Subject not found.");

        var items = students.Select((s, i) =>
        {
            byStudent.TryGetValue(s.Id, out var a);
            return new StudentAttendanceRowDto
            {
                Id = a?.Id,
                Sl = i + 1,
                StudentId = s.Id,
                StudentName = StudentName(s),
                RegisterNo = s.RegisterNo,
                Roll = s.Roll,
                Status = a?.Status ?? "Present",
                Remarks = a?.Remarks
            };
        }).ToList();

        return new SubjectAttendanceForDateResponseDto
        {
            ClassId = classId,
            SectionId = sectionId,
            SubjectId = subjectId,
            AttendanceDate = date.Date,
            SubjectName = string.IsNullOrWhiteSpace(subject.Code) ? subject.Name : $"{subject.Name} ({subject.Code})",
            Items = items
        };
    }

    public async Task<SubjectAttendanceForDateResponseDto> SaveAsync(SaveSubjectAttendanceDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        ManageOrTeacher();

        if (dto.Items.Count == 0)
            throw new AppException("At least one attendance record is required.", 400);

        foreach (var item in dto.Items)
        {
            if (!StudentAttendanceService.Statuses.Contains(item.Status, StringComparer.OrdinalIgnoreCase))
                throw new AppException($"Invalid attendance status '{item.Status}'.", 400);
        }

        _ = await uow.Subjects.GetByIdAsync(dto.SubjectId, ct)
            ?? throw new NotFoundException("Subject not found.");

        var userId = TryCurrentUser();
        var entries = dto.Items.Select(i => new StudentSubjectAttendance
        {
            Id = Guid.NewGuid(),
            StudentId = i.StudentId,
            ClassId = dto.ClassId,
            SectionId = dto.SectionId,
            SubjectId = dto.SubjectId,
            AttendanceDate = dto.AttendanceDate.Date,
            Status = StudentAttendanceService.Statuses.First(s => s.Equals(i.Status, StringComparison.OrdinalIgnoreCase)),
            Remarks = i.Remarks?.Trim(),
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }).ToList();

        await uow.BeginTenantTransactionAsync(ct);
        try
        {
            await uow.StudentSubjectAttendances.UpsertBatchAsync(entries, ct);
            await uow.SaveTenantChangesAsync(ct);
            await uow.CommitTenantTransactionAsync(ct);
        }
        catch
        {
            await uow.RollbackTenantTransactionAsync(ct);
            throw;
        }

        return await GetForDateAsync(dto.ClassId, dto.SectionId, dto.SubjectId, dto.AttendanceDate, ct);
    }

    private static string StudentName(Student s)
        => string.IsNullOrWhiteSpace(s.LastName) ? s.FirstName.Trim() : $"{s.FirstName.Trim()} {s.LastName.Trim()}";

    private async Task Ready(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(tenant.SchemaName))
            throw new AppException("X-Tenant-ID header is required.", 400);
        await provisioner.EnsureSettingsModuleAsync(tenant.SchemaName!, ct);
    }

    private HashSet<string> Roles() =>
        http.HttpContext?.User.FindAll("role").Concat(http.HttpContext.User.FindAll(ClaimTypes.Role))
            .Select(x => x.Value).ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];

    private void ManageOrTeacher()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin)
            && !r.Contains(AppConstants.Roles.Teacher))
            throw new ForbiddenException("Only Super Admin, School Admin, or Teacher can manage subject attendance.");
    }

    private Guid? TryCurrentUser()
    {
        var id = http.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? http.HttpContext?.User.FindFirst("sub")?.Value;
        return Guid.TryParse(id, out var g) ? g : null;
    }
}
