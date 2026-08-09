using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class FeesGroupItemRepository(TenantDbContext context) : IFeesGroupItemRepository
{
    public async Task<IReadOnlyList<FeesGroupItem>> GetByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = default)
        => await context.FeesGroupItems.Include(i => i.FeesType).Where(x => x.GroupId == groupId).OrderBy(x => x.SortOrder).ToListAsync(cancellationToken);

    public async Task<FeesGroupItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.FeesGroupItems.Include(i => i.FeesType).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<FeesGroupItem> AddAsync(FeesGroupItem entity, CancellationToken cancellationToken = default)
    {
        await context.FeesGroupItems.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(FeesGroupItem entity, CancellationToken cancellationToken = default)
    {
        context.FeesGroupItems.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(FeesGroupItem entity, CancellationToken cancellationToken = default)
    {
        context.FeesGroupItems.Remove(entity);
        return Task.CompletedTask;
    }
}
