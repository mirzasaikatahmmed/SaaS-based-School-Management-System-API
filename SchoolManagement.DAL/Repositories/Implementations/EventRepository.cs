using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class EventRepository(TenantDbContext context) : IEventRepository
{
    public async Task<(IReadOnlyList<SchoolEvent> Items, int TotalCount)> SearchAsync(EventSearchFilter filter, CancellationToken cancellationToken = default)
    {
        var q = context.Events
            .Include(e => e.EventType)
            .Include(e => e.CreatedByUser)
            .AsQueryable();

        if (filter.EventTypeId.HasValue)
            q = q.Where(e => e.EventTypeId == filter.EventTypeId.Value);
        if (filter.IsActive.HasValue)
            q = q.Where(e => e.IsActive == filter.IsActive.Value);
        if (filter.FromDate.HasValue)
            q = q.Where(e => e.DateOfEnd >= filter.FromDate.Value.Date);
        if (filter.ToDate.HasValue)
            q = q.Where(e => e.DateOfStart <= filter.ToDate.Value.Date);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            q = q.Where(e => e.Title.ToLower().Contains(s));
        }

        var total = await q.CountAsync(cancellationToken);
        var page = filter.Page < 1 ? 1 : filter.Page;
        var size = filter.PageSize is < 1 or > 200 ? 25 : filter.PageSize;
        var items = await q.OrderByDescending(e => e.DateOfStart)
            .Skip((page - 1) * size).Take(size)
            .ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task<SchoolEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Events
            .Include(e => e.EventType)
            .Include(e => e.CreatedByUser)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<IReadOnlyList<DateTime>> GetHolidayDatesAsync(
        DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        var from = fromDate.Date;
        var to = toDate.Date;
        var events = await context.Events
            .Where(e => e.IsActive && e.IsHoliday && e.DateOfEnd.Date >= from && e.DateOfStart.Date <= to)
            .Select(e => new { e.DateOfStart, e.DateOfEnd })
            .ToListAsync(cancellationToken);

        var dates = new HashSet<DateTime>();
        foreach (var ev in events)
        {
            var start = ev.DateOfStart.Date < from ? from : ev.DateOfStart.Date;
            var end = ev.DateOfEnd.Date > to ? to : ev.DateOfEnd.Date;
            for (var d = start; d <= end; d = d.AddDays(1))
                dates.Add(d);
        }

        return dates.OrderBy(d => d).ToList();
    }

    public async Task<IReadOnlyList<SchoolEvent>> GetPublicAsync(CancellationToken cancellationToken = default)
        => await context.Events
            .Include(e => e.EventType)
            .Where(e => e.IsActive && e.IsPublished)
            .OrderBy(e => e.DateOfStart)
            .ToListAsync(cancellationToken);

    public async Task<SchoolEvent> AddAsync(SchoolEvent entity, CancellationToken cancellationToken = default)
    {
        await context.Events.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(SchoolEvent entity, CancellationToken cancellationToken = default)
    {
        context.Events.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(SchoolEvent entity, CancellationToken cancellationToken = default)
    {
        context.Events.Remove(entity);
        return Task.CompletedTask;
    }
}
