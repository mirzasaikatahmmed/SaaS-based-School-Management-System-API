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

public class ClassScheduleService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http) : IClassScheduleService
{
    public async Task<IReadOnlyList<ClassScheduleResponseDto>> GetByClassSectionAsync(Guid classId, Guid sectionId, CancellationToken ct = default)
    {
        await Ready(ct);
        Read();
        return (await uow.ClassSchedules.GetByClassSectionAsync(classId, sectionId, ct)).Select(Map).ToList();
    }

    public async Task<IReadOnlyList<ClassScheduleResponseDto>> GetMyClassScheduleAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        var userId = CurrentUser();
        var student = await uow.Students.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException("No student profile found for current user.");
        if (!student.ClassId.HasValue || !student.SectionId.HasValue)
            return [];
        return (await uow.ClassSchedules.GetByClassSectionAsync(student.ClassId.Value, student.SectionId.Value, ct)).Select(Map).ToList();
    }

    public async Task<ClassScheduleResponseDto> UpsertAsync(UpsertClassScheduleDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();

        var day = WeekDays.All.FirstOrDefault(d => d.Equals(dto.Day?.Trim(), StringComparison.OrdinalIgnoreCase))
            ?? throw new AppException("Invalid day of week.", 400);

        foreach (var p in dto.Periods)
        {
            if (p.EndingTime <= p.StartingTime)
                throw new AppException("Period ending time must be after starting time.", 400);
        }

        var existing = await uow.ClassSchedules.GetByClassSectionDayAsync(dto.ClassId, dto.SectionId, day, ct);
        Guid scheduleId;
        if (existing is not null)
        {
            existing.UpdatedAt = DateTime.UtcNow;
            await uow.ClassSchedules.UpdateAsync(existing, ct);
            scheduleId = existing.Id;
        }
        else
        {
            var entity = new ClassSchedule
            {
                Id = Guid.NewGuid(),
                ClassId = dto.ClassId,
                SectionId = dto.SectionId,
                Day = day,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await uow.ClassSchedules.AddAsync(entity, ct);
            scheduleId = entity.Id;
        }

        var periods = dto.Periods.Select((p, i) => new ClassSchedulePeriod
        {
            Id = Guid.NewGuid(),
            ScheduleId = scheduleId,
            IsBreak = p.IsBreak,
            SubjectId = p.IsBreak ? null : p.SubjectId,
            EmployeeId = p.IsBreak ? null : p.EmployeeId,
            StartingTime = p.StartingTime,
            EndingTime = p.EndingTime,
            ClassRoom = string.IsNullOrWhiteSpace(p.ClassRoom) ? null : p.ClassRoom.Trim(),
            SortOrder = p.SortOrder != 0 ? p.SortOrder : i,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        await uow.ClassSchedules.ReplacePeriodsAsync(scheduleId, periods, ct);
        await uow.SaveTenantChangesAsync(ct);

        var result = await uow.ClassSchedules.GetByIdAsync(scheduleId, ct)
            ?? throw new AppException("Failed to load saved class schedule.", 500);
        return Map(result);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var x = await uow.ClassSchedules.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Class schedule '{id}' not found.");
        await uow.ClassSchedules.DeleteAsync(x, ct);
        await uow.SaveTenantChangesAsync(ct);
    }

    public async Task<IReadOnlyList<TeacherScheduleDayDto>> GetTeacherScheduleAsync(Guid employeeId, CancellationToken ct = default)
    {
        await Ready(ct);
        ManageOrTeacher();
        var periods = await uow.ClassSchedules.GetByTeacherAsync(employeeId, ct);
        return MapTeacherSchedule(periods);
    }

    public async Task<IReadOnlyList<TeacherScheduleDayDto>> GetMyTeacherScheduleAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        var userId = CurrentUser();
        var employee = await uow.Employees.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException("No employee profile found for current user.");
        var periods = await uow.ClassSchedules.GetByTeacherAsync(employee.Id, ct);
        return MapTeacherSchedule(periods);
    }

    private static IReadOnlyList<TeacherScheduleDayDto> MapTeacherSchedule(IReadOnlyList<ClassSchedulePeriod> periods)
        => WeekDays.All
            .Select(day => new TeacherScheduleDayDto
            {
                Day = day,
                Periods = periods
                    .Where(p => p.Schedule.Day == day)
                    .OrderBy(p => p.SortOrder)
                    .Select(p => new TeacherSchedulePeriodDto
                    {
                        ClassName = p.Schedule.Class.Name,
                        SectionName = p.Schedule.Section.Name,
                        SubjectName = p.Subject?.Name,
                        StartingTime = p.StartingTime,
                        EndingTime = p.EndingTime,
                        ClassRoom = p.ClassRoom,
                        IsBreak = p.IsBreak
                    }).ToList()
            })
            .Where(d => d.Periods.Count > 0)
            .ToList();

    private static ClassScheduleResponseDto Map(ClassSchedule x) => new()
    {
        Id = x.Id,
        ClassId = x.ClassId,
        ClassName = x.Class?.Name ?? string.Empty,
        SectionId = x.SectionId,
        SectionName = x.Section?.Name ?? string.Empty,
        Day = x.Day,
        Periods = x.Periods.OrderBy(p => p.SortOrder).Select(p => new SchedulePeriodResponseDto
        {
            Id = p.Id,
            IsBreak = p.IsBreak,
            SubjectId = p.SubjectId,
            SubjectName = p.Subject?.Name,
            EmployeeId = p.EmployeeId,
            EmployeeName = p.Employee?.Name,
            StartingTime = p.StartingTime,
            EndingTime = p.EndingTime,
            ClassRoom = p.ClassRoom,
            SortOrder = p.SortOrder
        }).ToList()
    };

    private async Task Ready(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(tenant.SchemaName))
            throw new AppException("X-Tenant-ID header is required.", 400);
        await provisioner.EnsureEmployeeModuleAsync(tenant.SchemaName!, ct);
    }

    private HashSet<string> Roles()
    {
        var p = http.HttpContext?.User;
        if (p is null) return [];
        return p.FindAll("role").Concat(p.FindAll(ClaimTypes.Role))
            .Select(x => x.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private void Manage()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin))
            throw new ForbiddenException("Only Super Admin or School Admin can manage class schedules.");
    }

    private void Read()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin) &&
            !r.Contains(AppConstants.Roles.Teacher) && !r.Contains(AppConstants.Roles.Student))
            throw new ForbiddenException("You do not have access to class schedules.");
    }

    private void ManageOrTeacher()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin) &&
            !r.Contains(AppConstants.Roles.Teacher))
            throw new ForbiddenException("You do not have access to teacher schedules.");
    }

    private Guid CurrentUser()
    {
        var c = http.HttpContext?.User.FindFirst(AppConstants.Claims.UserId)
            ?? http.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        if (c is null || !Guid.TryParse(c.Value, out var id)) throw new UnauthorizedException();
        return id;
    }
}
