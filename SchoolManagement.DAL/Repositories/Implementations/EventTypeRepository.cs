using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class EventTypeRepository(TenantDbContext context) : IEventTypeRepository
{
    public async Task<IReadOnlyList<EventType>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.EventTypes.OrderBy(t => t.Name).ToListAsync(cancellationToken);

    public async Task<EventType?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.EventTypes.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var n = name.Trim().ToUpperInvariant();
        var q = context.EventTypes.Where(t => t.Name.ToUpper() == n);
        if (excludeId.HasValue) q = q.Where(t => t.Id != excludeId.Value);
        return await q.AnyAsync(cancellationToken);
    }

    public async Task<int> CountEventsUsingAsync(Guid eventTypeId, CancellationToken cancellationToken = default)
        => await context.Events.CountAsync(e => e.EventTypeId == eventTypeId, cancellationToken);

    public async Task<EventType> AddAsync(EventType entity, CancellationToken cancellationToken = default)
    {
        await context.EventTypes.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(EventType entity, CancellationToken cancellationToken = default)
    {
        context.EventTypes.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(EventType entity, CancellationToken cancellationToken = default)
    {
        context.EventTypes.Remove(entity);
        return Task.CompletedTask;
    }
}
