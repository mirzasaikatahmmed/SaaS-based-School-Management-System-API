using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class SmsSettingsRepository(TenantDbContext context) : ISmsSettingsRepository
{
    public async Task<SmsSettings> GetOrCreateAsync(CancellationToken cancellationToken = default)
    {
        var existing = await context.SmsSettings.FirstOrDefaultAsync(cancellationToken);
        if (existing is not null) return existing;

        var created = new SmsSettings { Id = Guid.NewGuid() };
        await context.SmsSettings.AddAsync(created, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return created;
    }

    public Task UpdateAsync(SmsSettings entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        context.SmsSettings.Update(entity);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<SmsTemplate>> GetTemplatesAsync(CancellationToken cancellationToken = default)
        => await context.SmsTemplates.OrderBy(t => t.Name).ToListAsync(cancellationToken);

    public async Task<SmsTemplate?> GetTemplateAsync(string eventKey, CancellationToken cancellationToken = default)
        => await context.SmsTemplates.FirstOrDefaultAsync(t => t.EventKey == eventKey, cancellationToken);

    public async Task UpsertTemplateAsync(SmsTemplate template, CancellationToken cancellationToken = default)
    {
        var existing = await context.SmsTemplates
            .FirstOrDefaultAsync(t => t.EventKey == template.EventKey, cancellationToken);
        if (existing is null)
        {
            template.Id = Guid.NewGuid();
            template.CreatedAt = DateTime.UtcNow;
            template.UpdatedAt = DateTime.UtcNow;
            await context.SmsTemplates.AddAsync(template, cancellationToken);
        }
        else
        {
            existing.Name = template.Name;
            existing.Body = template.Body;
            existing.NotifyStudent = template.NotifyStudent;
            existing.NotifyParent = template.NotifyParent;
            existing.DltTemplateId = template.DltTemplateId;
            existing.NotifyEnabled = template.NotifyEnabled;
            existing.UpdatedAt = DateTime.UtcNow;
        }
    }
}
