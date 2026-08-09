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

public class ExamAttendanceService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http) : IExamAttendanceService
{
    public static readonly string[] Statuses = ["Present", "Absent", "Late"];

    public async Task<ExamAttendanceResponseDto> GetAsync(ExamAttendanceFilterDto filter, CancellationToken ct = default)
    {
        await Ready(ct);
        ManageOrTeacher();

        var students = await uow.ExamAttendances.GetActiveStudentsAsync(filter.ClassId, filter.SectionId, ct);
        var existing = await uow.ExamAttendances.GetForFilterAsync(filter.ExamId, filter.ClassId, filter.SectionId, filter.SubjectId, ct);
        var byStudent = existing.ToDictionary(a => a.StudentId);

        var items = students.Select((s, i) =>
        {
            byStudent.TryGetValue(s.Id, out var a);
            return new ExamAttendanceRowDto
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

        return new ExamAttendanceResponseDto
        {
            ExamId = filter.ExamId,
            ClassId = filter.ClassId,
            SectionId = filter.SectionId,
            SubjectId = filter.SubjectId,
            Items = items
        };
    }

    public async Task<ExamAttendanceResponseDto> SaveAsync(SaveExamAttendanceDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        ManageOrTeacher();

        if (dto.Items.Count == 0)
            throw new AppException("At least one attendance record is required.", 400);

        foreach (var item in dto.Items)
        {
            if (!Statuses.Contains(item.Status, StringComparer.OrdinalIgnoreCase))
                throw new AppException($"Invalid attendance status '{item.Status}'.", 400);
        }

        var entries = dto.Items.Select(i => new ExamAttendance
        {
            Id = Guid.NewGuid(),
            ExamId = dto.ExamId,
            ClassId = dto.ClassId,
            SectionId = dto.SectionId,
            SubjectId = dto.SubjectId,
            StudentId = i.StudentId,
            Status = Statuses.First(s => s.Equals(i.Status, StringComparison.OrdinalIgnoreCase)),
            Remarks = i.Remarks?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }).ToList();

        await uow.BeginTenantTransactionAsync(ct);
        try
        {
            await uow.ExamAttendances.UpsertBatchAsync(entries, ct);
            await uow.SaveTenantChangesAsync(ct);
            await uow.CommitTenantTransactionAsync(ct);
        }
        catch
        {
            await uow.RollbackTenantTransactionAsync(ct);
            throw;
        }

        return await GetAsync(new ExamAttendanceFilterDto
        {
            ExamId = dto.ExamId,
            ClassId = dto.ClassId,
            SectionId = dto.SectionId,
            SubjectId = dto.SubjectId
        }, ct);
    }

    private static string StudentName(Student s)
        => string.IsNullOrWhiteSpace(s.LastName) ? s.FirstName.Trim() : $"{s.FirstName.Trim()} {s.LastName.Trim()}";

    private async Task Ready(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(tenant.SchemaName))
            throw new AppException("X-Tenant-ID header is required.", 400);
        await provisioner.EnsureGradesAttendanceLibraryEventsModuleAsync(tenant.SchemaName!, ct);
    }

    private HashSet<string> Roles()
    {
        var p = http.HttpContext?.User;
        if (p is null) return [];
        return p.FindAll("role").Concat(p.FindAll(ClaimTypes.Role)).Select(x => x.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private void ManageOrTeacher()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin) &&
            !r.Contains(AppConstants.Roles.Teacher))
            throw new ForbiddenException("Only Super Admin, School Admin, or Teacher can manage exam attendance.");
    }
}
