using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Master;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class CronSecretRegistryRepository(MasterDbContext context) : ICronSecretRegistryRepository
{
    public async Task<CronSecretRegistry?> GetBySecretKeyAsync(string secretKey, CancellationToken cancellationToken = default)
        => await context.CronSecretRegistries
            .Include(r => r.Tenant)
            .FirstOrDefaultAsync(r => r.SecretKey == secretKey, cancellationToken);

    public async Task<CronSecretRegistry?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
        => await context.CronSecretRegistries.FirstOrDefaultAsync(r => r.TenantId == tenantId, cancellationToken);

    public async Task UpsertAsync(Guid tenantId, string schemaName, string secretKey, CancellationToken cancellationToken = default)
    {
        var existing = await context.CronSecretRegistries.FirstOrDefaultAsync(r => r.TenantId == tenantId, cancellationToken);
        if (existing is null)
        {
            await context.CronSecretRegistries.AddAsync(new CronSecretRegistry
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                SchemaName = schemaName,
                SecretKey = secretKey,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }, cancellationToken);
        }
        else
        {
            existing.SchemaName = schemaName;
            existing.SecretKey = secretKey;
            existing.UpdatedAt = DateTime.UtcNow;
        }
    }
}
