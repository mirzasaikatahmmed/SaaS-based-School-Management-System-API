using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class MarkDistributionRepository(TenantDbContext context) : IMarkDistributionRepository
{
    public async Task<IReadOnlyList<MarkDistribution>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.MarkDistributions.OrderBy(d => d.Name).ToListAsync(cancellationToken);

    public async Task<MarkDistribution?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.MarkDistributions.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var n = name.Trim().ToUpperInvariant();
        var q = context.MarkDistributions.Where(d => d.Name.ToUpper() == n);
        if (excludeId.HasValue) q = q.Where(d => d.Id != excludeId.Value);
        return await q.AnyAsync(cancellationToken);
    }

    public async Task<int> CountExamsUsingAsync(Guid distributionId, CancellationToken cancellationToken = default)
        => await context.ExamMarkDistributions.CountAsync(e => e.MarkDistributionId == distributionId, cancellationToken);

    public async Task<MarkDistribution> AddAsync(MarkDistribution entity, CancellationToken cancellationToken = default)
    {
        await context.MarkDistributions.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(MarkDistribution entity, CancellationToken cancellationToken = default)
    {
        context.MarkDistributions.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(MarkDistribution entity, CancellationToken cancellationToken = default)
    {
        context.MarkDistributions.Remove(entity);
        return Task.CompletedTask;
    }
}
