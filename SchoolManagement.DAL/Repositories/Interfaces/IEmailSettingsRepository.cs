using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IEmailSettingsRepository
{
    Task<EmailSettings> GetOrCreateAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(EmailSettings entity, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmailTemplate>> GetTemplatesAsync(CancellationToken cancellationToken = default);
    Task<EmailTemplate?> GetTemplateAsync(string eventKey, CancellationToken cancellationToken = default);
    Task UpsertTemplateAsync(EmailTemplate template, CancellationToken cancellationToken = default);
}
