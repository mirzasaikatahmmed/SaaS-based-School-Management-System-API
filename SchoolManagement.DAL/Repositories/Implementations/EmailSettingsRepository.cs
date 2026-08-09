using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class EmailSettingsRepository(TenantDbContext context) : IEmailSettingsRepository
{
    public async Task<EmailSettings> GetOrCreateAsync(CancellationToken cancellationToken = default)
    {
        var existing = await context.EmailSettings.FirstOrDefaultAsync(cancellationToken);
        if (existing is not null) return existing;

        var created = new EmailSettings { Id = Guid.NewGuid() };
        await context.EmailSettings.AddAsync(created, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return created;
    }

    public Task UpdateAsync(EmailSettings entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        context.EmailSettings.Update(entity);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<EmailTemplate>> GetTemplatesAsync(CancellationToken cancellationToken = default)
        => await context.EmailTemplates.OrderBy(t => t.Name).ToListAsync(cancellationToken);

    public async Task<EmailTemplate?> GetTemplateAsync(string eventKey, CancellationToken cancellationToken = default)
        => await context.EmailTemplates.FirstOrDefaultAsync(t => t.EventKey == eventKey, cancellationToken);

    public async Task UpsertTemplateAsync(EmailTemplate template, CancellationToken cancellationToken = default)
    {
        var existing = await context.EmailTemplates
            .FirstOrDefaultAsync(t => t.EventKey == template.EventKey, cancellationToken);
        if (existing is null)
        {
            template.Id = Guid.NewGuid();
            template.CreatedAt = DateTime.UtcNow;
            template.UpdatedAt = DateTime.UtcNow;
            await context.EmailTemplates.AddAsync(template, cancellationToken);
        }
        else
        {
            existing.Name = template.Name;
            existing.Subject = template.Subject;
            existing.BodyHtml = template.BodyHtml;
            existing.NotifyEnabled = template.NotifyEnabled;
            existing.UpdatedAt = DateTime.UtcNow;
        }
    }
}
