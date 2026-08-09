using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class GradeRangeRepository(TenantDbContext context) : IGradeRangeRepository
{
    public async Task<IReadOnlyList<GradeRange>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.GradeRanges.OrderBy(g => g.SortOrder).ThenBy(g => g.MinPercentage).ToListAsync(cancellationToken);

    public async Task<GradeRange?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.GradeRanges.FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

    public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var n = name.Trim().ToUpperInvariant();
        var q = context.GradeRanges.Where(g => g.GradeName.ToUpper() == n);
        if (excludeId.HasValue) q = q.Where(g => g.Id != excludeId.Value);
        return await q.AnyAsync(cancellationToken);
    }

    public async Task<bool> OverlapsAsync(decimal minPercentage, decimal maxPercentage, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var q = context.GradeRanges.Where(g => g.MinPercentage <= maxPercentage && minPercentage <= g.MaxPercentage);
        if (excludeId.HasValue) q = q.Where(g => g.Id != excludeId.Value);
        return await q.AnyAsync(cancellationToken);
    }

    public Task<int> CountExamPositionsAsync(CancellationToken cancellationToken = default)
        => context.ExamPositions.CountAsync(cancellationToken);

    public async Task<GradeRange> AddAsync(GradeRange entity, CancellationToken cancellationToken = default)
    {
        await context.GradeRanges.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(GradeRange entity, CancellationToken cancellationToken = default)
    {
        context.GradeRanges.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(GradeRange entity, CancellationToken cancellationToken = default)
    {
        context.GradeRanges.Remove(entity);
        return Task.CompletedTask;
    }
}
