using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class SchoolSettingsRepository(TenantDbContext context) : ISchoolSettingsRepository
{
    public async Task<SchoolSettings?> GetAsync(CancellationToken cancellationToken = default)
        => await context.SchoolSettings.FirstOrDefaultAsync(cancellationToken);

    public async Task<SchoolSettings> GetOrCreateAsync(CancellationToken cancellationToken = default)
    {
        var existing = await context.SchoolSettings.FirstOrDefaultAsync(cancellationToken);
        if (existing is not null)
            return existing;

        var created = new SchoolSettings
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await context.SchoolSettings.AddAsync(created, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return created;
    }

    public Task UpdateAsync(SchoolSettings entity, CancellationToken cancellationToken = default)
    {
        context.SchoolSettings.Update(entity);
        return Task.CompletedTask;
    }
}
