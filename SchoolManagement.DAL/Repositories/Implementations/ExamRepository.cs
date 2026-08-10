using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class ExamRepository(TenantDbContext context) : IExamRepository
{
    private IQueryable<Exam> WithIncludes()
        => context.Exams
            .Include(e => e.ExamTerm)
            .Include(e => e.MarkDistributions).ThenInclude(md => md.MarkDistribution);

    public async Task<IReadOnlyList<Exam>> GetAllAsync(CancellationToken cancellationToken = default)
        => await WithIncludes().OrderByDescending(e => e.CreatedAt).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Exam>> GetResultPublishedAsync(CancellationToken cancellationToken = default)
        => await WithIncludes()
            .Where(e => e.IsActive && e.IsResultPublished)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<Exam?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await WithIncludes().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var n = name.Trim().ToUpperInvariant();
        var q = context.Exams.Where(e => e.Name.ToUpper() == n);
        if (excludeId.HasValue) q = q.Where(e => e.Id != excludeId.Value);
        return await q.AnyAsync(cancellationToken);
    }

    public async Task<int> CountSchedulesAsync(Guid examId, CancellationToken cancellationToken = default)
        => await context.ExamSchedules.CountAsync(s => s.ExamId == examId, cancellationToken);

    public async Task<int> CountMarkEntriesAsync(Guid examId, CancellationToken cancellationToken = default)
        => await context.MarkEntries.CountAsync(m => m.ExamId == examId, cancellationToken);

    public async Task<Exam> AddAsync(Exam entity, CancellationToken cancellationToken = default)
    {
        await context.Exams.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(Exam entity, CancellationToken cancellationToken = default)
    {
        context.Exams.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Exam entity, CancellationToken cancellationToken = default)
    {
        context.Exams.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task ReplaceMarkDistributionsAsync(Guid examId, IEnumerable<ExamMarkDistribution> distributions, CancellationToken cancellationToken = default)
    {
        var existing = await context.ExamMarkDistributions.Where(d => d.ExamId == examId).ToListAsync(cancellationToken);
        context.ExamMarkDistributions.RemoveRange(existing);
        await context.ExamMarkDistributions.AddRangeAsync(distributions, cancellationToken);
    }
}
