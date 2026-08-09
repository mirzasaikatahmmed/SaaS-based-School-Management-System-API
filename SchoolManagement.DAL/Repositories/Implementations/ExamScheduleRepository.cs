using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class ExamScheduleRepository(TenantDbContext context) : IExamScheduleRepository
{
    private IQueryable<ExamSchedule> WithIncludes()
        => context.ExamSchedules
            .Include(s => s.Exam).ThenInclude(e => e!.ExamTerm)
            .Include(s => s.Class)
            .Include(s => s.Section)
            .Include(s => s.Subjects).ThenInclude(ss => ss.Subject)
            .Include(s => s.Subjects).ThenInclude(ss => ss.Hall);

    public async Task<IReadOnlyList<ExamSchedule>> GetFilteredAsync(Guid? classId, Guid? sectionId, CancellationToken cancellationToken = default)
    {
        var q = WithIncludes().AsQueryable();
        if (classId.HasValue) q = q.Where(s => s.ClassId == classId.Value);
        if (sectionId.HasValue) q = q.Where(s => s.SectionId == sectionId.Value);
        return await q.OrderByDescending(s => s.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<ExamSchedule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await WithIncludes().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<bool> ExistsUniqueAsync(Guid examId, Guid classId, Guid sectionId, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var q = context.ExamSchedules.Where(s => s.ExamId == examId && s.ClassId == classId && s.SectionId == sectionId);
        if (excludeId.HasValue) q = q.Where(s => s.Id != excludeId.Value);
        return await q.AnyAsync(cancellationToken);
    }

    public async Task<ExamSchedule> AddAsync(ExamSchedule entity, CancellationToken cancellationToken = default)
    {
        await context.ExamSchedules.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(ExamSchedule entity, CancellationToken cancellationToken = default)
    {
        context.ExamSchedules.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ExamSchedule entity, CancellationToken cancellationToken = default)
    {
        context.ExamSchedules.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task ReplaceSubjectsAsync(Guid scheduleId, IEnumerable<ExamScheduleSubject> subjects, CancellationToken cancellationToken = default)
    {
        var existing = await context.ExamScheduleSubjects.Where(s => s.ScheduleId == scheduleId).ToListAsync(cancellationToken);
        context.ExamScheduleSubjects.RemoveRange(existing);
        await context.ExamScheduleSubjects.AddRangeAsync(subjects, cancellationToken);
    }

    public async Task<ExamScheduleSubject?> FindExamSubjectForPunchAsync(
        Guid classId, Guid sectionId, DateTime date, TimeSpan time,
        int graceBeforeMinutes, int graceAfterMinutes, CancellationToken cancellationToken = default)
    {
        var candidates = await context.ExamScheduleSubjects
            .Include(s => s.Schedule).ThenInclude(sc => sc.Exam)
            .Include(s => s.Schedule).ThenInclude(sc => sc.Class)
            .Include(s => s.Schedule).ThenInclude(sc => sc.Section)
            .Where(s => s.Schedule.ClassId == classId
                && s.Schedule.SectionId == sectionId
                && s.ExamDate.Date == date.Date)
            .ToListAsync(cancellationToken);

        var before = TimeSpan.FromMinutes(Math.Max(0, graceBeforeMinutes));
        var after = TimeSpan.FromMinutes(Math.Max(0, graceAfterMinutes));

        return candidates
            .Where(s => time >= s.StartingTime - before && time <= s.EndingTime + after)
            .OrderBy(s => Math.Abs((time - s.StartingTime).Ticks))
            .FirstOrDefault();
    }
}
