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

public class StudentAttendanceService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http) : IStudentAttendanceService
{
    public static readonly string[] Statuses = ["Present", "Absent", "Late", "HalfDay"];

    public async Task<StudentAttendanceForDateResponseDto> GetForDateAsync(Guid classId, Guid sectionId, DateTime date, CancellationToken ct = default)
    {
        await Ready(ct);
        ManageOrTeacher();

        var students = await uow.StudentAttendances.GetActiveStudentsAsync(classId, sectionId, ct);
        var existing = await uow.StudentAttendances.GetForDateAsync(classId, sectionId, date, ct);
        var byStudent = existing.ToDictionary(a => a.StudentId);

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

        return new StudentAttendanceForDateResponseDto
        {
            ClassId = classId,
            SectionId = sectionId,
            AttendanceDate = date.Date,
            Items = items
        };
    }

    public async Task<StudentAttendanceForDateResponseDto> SaveAsync(SaveStudentAttendanceDto dto, CancellationToken ct = default)
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

        var userId = TryCurrentUser();
        var entries = dto.Items.Select(i => new StudentAttendance
        {
            Id = Guid.NewGuid(),
            StudentId = i.StudentId,
            ClassId = dto.ClassId,
            SectionId = dto.SectionId,
            AttendanceDate = dto.AttendanceDate.Date,
            Status = Statuses.First(s => s.Equals(i.Status, StringComparison.OrdinalIgnoreCase)),
            Remarks = i.Remarks?.Trim(),
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }).ToList();

        await uow.BeginTenantTransactionAsync(ct);
        try
        {
            await uow.StudentAttendances.UpsertBatchAsync(entries, ct);
            await uow.SaveTenantChangesAsync(ct);
            await uow.CommitTenantTransactionAsync(ct);
        }
        catch
        {
            await uow.RollbackTenantTransactionAsync(ct);
            throw;
        }

        return await GetForDateAsync(dto.ClassId, dto.SectionId, dto.AttendanceDate, ct);
    }

    public async Task<StudentAttendanceReportResponseDto> GetReportAsync(StudentAttendanceReportFilterDto filter, CancellationToken ct = default)
    {
        await Ready(ct);
        ManageOrTeacher();
        return await BuildReport(filter.ClassId, filter.SectionId, null, filter.FromDate, filter.ToDate, ct);
    }

    public async Task<StudentAttendanceReportResponseDto> GetMyReportAsync(StudentAttendanceReportFilterDto filter, CancellationToken ct = default)
    {
        await Ready(ct);
        var student = await uow.Students.GetByUserIdAsync(CurrentUser(), ct)
            ?? throw new NotFoundException("No student profile found for current user.");
        return await BuildReport(null, null, student.Id, filter.FromDate, filter.ToDate, ct);
    }

    private async Task<StudentAttendanceReportResponseDto> BuildReport(
        Guid? classId, Guid? sectionId, Guid? studentId, DateTime fromDate, DateTime toDate, CancellationToken ct)
    {
        var records = await uow.StudentAttendances.GetReportAsync(classId, sectionId, studentId, fromDate, toDate, ct);

        var rows = records.Select(a => new StudentAttendanceReportRowDto
        {
            AttendanceDate = a.AttendanceDate,
            StudentId = a.StudentId,
            StudentName = StudentName(a.Student),
            RegisterNo = a.Student.RegisterNo,
            Roll = a.Student.Roll,
            Status = a.Status,
            Remarks = a.Remarks
        }).ToList();

        var summary = records.GroupBy(a => a.StudentId).Select(g =>
        {
            var first = g.First();
            return new StudentAttendanceSummaryDto
            {
                StudentId = g.Key,
                StudentName = StudentName(first.Student),
                RegisterNo = first.Student.RegisterNo,
                PresentCount = g.Count(x => x.Status == "Present"),
                AbsentCount = g.Count(x => x.Status == "Absent"),
                LateCount = g.Count(x => x.Status == "Late"),
                HalfDayCount = g.Count(x => x.Status == "HalfDay"),
                TotalDays = g.Count()
            };
        }).ToList();

        return new StudentAttendanceReportResponseDto { Rows = rows, Summary = summary };
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
            throw new ForbiddenException("Only Super Admin, School Admin, or Teacher can manage student attendance.");
    }

    private Guid CurrentUser()
    {
        var c = http.HttpContext?.User.FindFirst(AppConstants.Claims.UserId)
            ?? http.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        if (c is null || !Guid.TryParse(c.Value, out var id)) throw new UnauthorizedException();
        return id;
    }

    private Guid? TryCurrentUser()
    {
        var c = http.HttpContext?.User.FindFirst(AppConstants.Claims.UserId)
            ?? http.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        return c is not null && Guid.TryParse(c.Value, out var id) ? id : null;
    }
}
