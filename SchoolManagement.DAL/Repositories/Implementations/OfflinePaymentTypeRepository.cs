using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class OfflinePaymentTypeRepository(TenantDbContext context) : IOfflinePaymentTypeRepository
{
    public async Task<IReadOnlyList<OfflinePaymentType>> GetAllAsync(bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var q = context.OfflinePaymentTypes.AsQueryable();
        if (isActive.HasValue) q = q.Where(x => x.IsActive == isActive.Value);
        return await q.OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }

    public async Task<OfflinePaymentType?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.OfflinePaymentTypes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
        => await context.OfflinePaymentTypes.AnyAsync(x => x.Name.ToLower() == name.ToLower() && (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);

    public async Task<OfflinePaymentType> AddAsync(OfflinePaymentType entity, CancellationToken cancellationToken = default)
    {
        await context.OfflinePaymentTypes.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(OfflinePaymentType entity, CancellationToken cancellationToken = default)
    {
        context.OfflinePaymentTypes.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(OfflinePaymentType entity, CancellationToken cancellationToken = default)
    {
        context.OfflinePaymentTypes.Remove(entity);
        return Task.CompletedTask;
    }
}
