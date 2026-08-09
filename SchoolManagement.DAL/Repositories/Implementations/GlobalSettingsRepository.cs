using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Master;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class GlobalSettingsRepository(MasterDbContext context) : IGlobalSettingsRepository
{
    public async Task<GlobalSettings?> GetAsync(CancellationToken cancellationToken = default)
        => await context.GlobalSettings.FirstOrDefaultAsync(cancellationToken);

    public async Task<GlobalSettings> AddAsync(GlobalSettings entity, CancellationToken cancellationToken = default)
    {
        await context.GlobalSettings.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(GlobalSettings entity, CancellationToken cancellationToken = default)
    {
        context.GlobalSettings.Update(entity);
        return Task.CompletedTask;
    }
}
