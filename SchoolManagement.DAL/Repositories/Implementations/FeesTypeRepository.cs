using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class FeesTypeRepository(TenantDbContext context) : IFeesTypeRepository
{
    public async Task<IReadOnlyList<FeesType>> GetAllAsync(bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var q = context.FeesTypes.AsQueryable();
        if (isActive.HasValue) q = q.Where(x => x.IsActive == isActive.Value);
        return await q.OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }

    public async Task<FeesType?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.FeesTypes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<bool> FeeCodeExistsAsync(string feeCode, Guid? excludeId = null, CancellationToken cancellationToken = default)
        => await context.FeesTypes.AnyAsync(x => x.FeeCode == feeCode && (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);

    public async Task<int> CountGroupItemsUsingAsync(Guid feesTypeId, CancellationToken cancellationToken = default)
        => await context.FeesGroupItems.CountAsync(x => x.FeesTypeId == feesTypeId, cancellationToken);

    public async Task<FeesType> AddAsync(FeesType entity, CancellationToken cancellationToken = default)
    {
        await context.FeesTypes.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(FeesType entity, CancellationToken cancellationToken = default)
    {
        context.FeesTypes.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(FeesType entity, CancellationToken cancellationToken = default)
    {
        context.FeesTypes.Remove(entity);
        return Task.CompletedTask;
    }
}
