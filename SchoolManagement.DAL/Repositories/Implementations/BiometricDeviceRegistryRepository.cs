using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Master;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class BiometricDeviceRegistryRepository(MasterDbContext context) : IBiometricDeviceRegistryRepository
{
    public async Task<BiometricDeviceRegistry?> GetBySerialNumberAsync(string serialNumber, CancellationToken cancellationToken = default)
        => await context.BiometricDeviceRegistries
            .Include(r => r.Tenant)
            .FirstOrDefaultAsync(r => r.SerialNumber == serialNumber, cancellationToken);

    public async Task<bool> SerialNumberExistsAsync(string serialNumber, CancellationToken cancellationToken = default)
        => await context.BiometricDeviceRegistries.AnyAsync(r => r.SerialNumber == serialNumber, cancellationToken);

    public async Task<BiometricDeviceRegistry> AddAsync(BiometricDeviceRegistry entity, CancellationToken cancellationToken = default)
    {
        await context.BiometricDeviceRegistries.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(BiometricDeviceRegistry entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        context.BiometricDeviceRegistries.Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteBySerialNumberAsync(string serialNumber, CancellationToken cancellationToken = default)
    {
        var existing = await context.BiometricDeviceRegistries
            .FirstOrDefaultAsync(r => r.SerialNumber == serialNumber, cancellationToken);
        if (existing is not null)
            context.BiometricDeviceRegistries.Remove(existing);
    }
}
