using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class ClassScheduleRepository(TenantDbContext context) : IClassScheduleRepository
{
    public async Task<IReadOnlyList<ClassSchedule>> GetByClassSectionAsync(Guid classId, Guid sectionId, CancellationToken cancellationToken = default)
        => await context.ClassSchedules
            .Include(s => s.Class)
            .Include(s => s.Section)
            .Include(s => s.Periods.OrderBy(p => p.SortOrder)).ThenInclude(p => p.Subject)
            .Include(s => s.Periods).ThenInclude(p => p.Employee)
            .Where(s => s.ClassId == classId && s.SectionId == sectionId)
            .ToListAsync(cancellationToken);

    public async Task<ClassSchedule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.ClassSchedules
            .Include(s => s.Class)
            .Include(s => s.Section)
            .Include(s => s.Periods.OrderBy(p => p.SortOrder)).ThenInclude(p => p.Subject)
            .Include(s => s.Periods).ThenInclude(p => p.Employee)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<ClassSchedule?> GetByClassSectionDayAsync(Guid classId, Guid sectionId, string day, CancellationToken cancellationToken = default)
        => await context.ClassSchedules
            .Include(s => s.Class)
            .Include(s => s.Section)
            .Include(s => s.Periods.OrderBy(p => p.SortOrder)).ThenInclude(p => p.Subject)
            .Include(s => s.Periods).ThenInclude(p => p.Employee)
            .FirstOrDefaultAsync(s => s.ClassId == classId && s.SectionId == sectionId && s.Day == day, cancellationToken);

    public async Task<ClassSchedule> AddAsync(ClassSchedule entity, CancellationToken cancellationToken = default)
    {
        await context.ClassSchedules.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(ClassSchedule entity, CancellationToken cancellationToken = default)
    {
        context.ClassSchedules.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ClassSchedule entity, CancellationToken cancellationToken = default)
    {
        context.ClassSchedules.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task ReplacePeriodsAsync(Guid scheduleId, IEnumerable<ClassSchedulePeriod> periods, CancellationToken cancellationToken = default)
    {
        var existing = await context.ClassSchedulePeriods.Where(p => p.ScheduleId == scheduleId).ToListAsync(cancellationToken);
        context.ClassSchedulePeriods.RemoveRange(existing);
        await context.ClassSchedulePeriods.AddRangeAsync(periods, cancellationToken);
    }

    public async Task<IReadOnlyList<ClassSchedulePeriod>> GetByTeacherAsync(Guid employeeId, CancellationToken cancellationToken = default)
        => await context.ClassSchedulePeriods
            .Include(p => p.Schedule).ThenInclude(s => s.Class)
            .Include(p => p.Schedule).ThenInclude(s => s.Section)
            .Include(p => p.Subject)
            .Where(p => p.EmployeeId == employeeId)
            .OrderBy(p => p.Schedule.Day).ThenBy(p => p.SortOrder)
            .ToListAsync(cancellationToken);
}
