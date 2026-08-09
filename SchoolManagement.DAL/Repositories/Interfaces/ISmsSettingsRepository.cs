using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface ISmsSettingsRepository
{
    Task<SmsSettings> GetOrCreateAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(SmsSettings entity, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SmsTemplate>> GetTemplatesAsync(CancellationToken cancellationToken = default);
    Task<SmsTemplate?> GetTemplateAsync(string eventKey, CancellationToken cancellationToken = default);
    Task UpsertTemplateAsync(SmsTemplate template, CancellationToken cancellationToken = default);
}
