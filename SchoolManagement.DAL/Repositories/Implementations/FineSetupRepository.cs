using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class FineSetupRepository(TenantDbContext context) : IFineSetupRepository
{
    public async Task<IReadOnlyList<FineSetup>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.FineSetups
            .Include(f => f.Group)
            .Include(f => f.FeesType)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<FineSetup?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.FineSetups
            .Include(f => f.Group)
            .Include(f => f.FeesType)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<bool> ExistsAsync(Guid groupId, Guid feesTypeId, Guid? excludeId = null, CancellationToken cancellationToken = default)
        => await context.FineSetups.AnyAsync(x =>
            x.GroupId == groupId && x.FeesTypeId == feesTypeId && (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);

    public async Task<FineSetup> AddAsync(FineSetup entity, CancellationToken cancellationToken = default)
    {
        await context.FineSetups.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(FineSetup entity, CancellationToken cancellationToken = default)
    {
        context.FineSetups.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(FineSetup entity, CancellationToken cancellationToken = default)
    {
        context.FineSetups.Remove(entity);
        return Task.CompletedTask;
    }
}
