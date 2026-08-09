using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class ExamTermRepository(TenantDbContext context) : IExamTermRepository
{
    public async Task<IReadOnlyList<ExamTerm>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.ExamTerms.OrderBy(t => t.CreatedAt).ToListAsync(cancellationToken);

    public async Task<ExamTerm?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.ExamTerms.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var n = name.Trim().ToUpperInvariant();
        var q = context.ExamTerms.Where(t => t.Name.ToUpper() == n);
        if (excludeId.HasValue) q = q.Where(t => t.Id != excludeId.Value);
        return await q.AnyAsync(cancellationToken);
    }

    public async Task<int> CountExamsUsingAsync(Guid termId, CancellationToken cancellationToken = default)
        => await context.Exams.CountAsync(e => e.ExamTermId == termId, cancellationToken);

    public async Task<ExamTerm> AddAsync(ExamTerm entity, CancellationToken cancellationToken = default)
    {
        await context.ExamTerms.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(ExamTerm entity, CancellationToken cancellationToken = default)
    {
        context.ExamTerms.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ExamTerm entity, CancellationToken cancellationToken = default)
    {
        context.ExamTerms.Remove(entity);
        return Task.CompletedTask;
    }
}
