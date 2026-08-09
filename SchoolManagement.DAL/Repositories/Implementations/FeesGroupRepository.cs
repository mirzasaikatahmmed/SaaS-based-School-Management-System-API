using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class FeesGroupRepository(TenantDbContext context) : IFeesGroupRepository
{
    public async Task<IReadOnlyList<FeesGroup>> GetAllAsync(bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var q = context.FeesGroups
            .Include(g => g.Items).ThenInclude(i => i.FeesType)
            .AsQueryable();
        if (isActive.HasValue) q = q.Where(x => x.IsActive == isActive.Value);
        return await q.OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }

    public async Task<FeesGroup?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.FeesGroups
            .Include(g => g.Items).ThenInclude(i => i.FeesType)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
        => await context.FeesGroups.AnyAsync(x => x.Name.ToLower() == name.ToLower() && (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);

    public async Task<int> CountAllocationsUsingAsync(Guid groupId, CancellationToken cancellationToken = default)
        => await context.FeesAllocations.CountAsync(x => x.FeesGroupId == groupId, cancellationToken);

    public async Task<FeesGroup> AddAsync(FeesGroup entity, CancellationToken cancellationToken = default)
    {
        await context.FeesGroups.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(FeesGroup entity, CancellationToken cancellationToken = default)
    {
        context.FeesGroups.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(FeesGroup entity, CancellationToken cancellationToken = default)
    {
        context.FeesGroups.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task ReplaceItemsAsync(Guid groupId, IEnumerable<FeesGroupItem> items, CancellationToken cancellationToken = default)
    {
        var existing = await context.FeesGroupItems.Where(x => x.GroupId == groupId).ToListAsync(cancellationToken);
        context.FeesGroupItems.RemoveRange(existing);
        await context.FeesGroupItems.AddRangeAsync(items, cancellationToken);
    }
}
