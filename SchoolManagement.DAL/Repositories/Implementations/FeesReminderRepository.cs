using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class FeesReminderRepository(TenantDbContext context) : IFeesReminderRepository
{
    public async Task<IReadOnlyList<FeesReminder>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.FeesReminders.OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);

    public async Task<FeesReminder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.FeesReminders.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<FeesReminder> AddAsync(FeesReminder entity, CancellationToken cancellationToken = default)
    {
        await context.FeesReminders.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(FeesReminder entity, CancellationToken cancellationToken = default)
    {
        context.FeesReminders.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(FeesReminder entity, CancellationToken cancellationToken = default)
    {
        context.FeesReminders.Remove(entity);
        return Task.CompletedTask;
    }
}
