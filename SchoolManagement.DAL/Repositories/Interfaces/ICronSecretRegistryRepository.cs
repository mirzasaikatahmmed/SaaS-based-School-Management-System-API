using SchoolManagement.DAL.Entities.Master;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface ICronSecretRegistryRepository
{
    Task<CronSecretRegistry?> GetBySecretKeyAsync(string secretKey, CancellationToken cancellationToken = default);
    Task<CronSecretRegistry?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task UpsertAsync(Guid tenantId, string schemaName, string secretKey, CancellationToken cancellationToken = default);
}
