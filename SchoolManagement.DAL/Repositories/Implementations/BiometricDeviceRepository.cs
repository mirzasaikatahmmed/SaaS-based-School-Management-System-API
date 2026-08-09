using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class BiometricDeviceRepository(TenantDbContext context) : IBiometricDeviceRepository
{
    public async Task<IReadOnlyList<BiometricDevice>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.BiometricDevices.OrderBy(d => d.Name).ToListAsync(cancellationToken);

    public async Task<BiometricDevice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.BiometricDevices.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public async Task<BiometricDevice?> GetBySerialNumberAsync(string serialNumber, CancellationToken cancellationToken = default)
        => await context.BiometricDevices.FirstOrDefaultAsync(d => d.SerialNumber == serialNumber, cancellationToken);

    public async Task<bool> SerialNumberExistsAsync(string serialNumber, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var q = context.BiometricDevices.Where(d => d.SerialNumber == serialNumber);
        if (excludeId.HasValue) q = q.Where(d => d.Id != excludeId.Value);
        return await q.AnyAsync(cancellationToken);
    }

    public async Task<BiometricDevice> AddAsync(BiometricDevice entity, CancellationToken cancellationToken = default)
    {
        await context.BiometricDevices.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(BiometricDevice entity, CancellationToken cancellationToken = default)
    {
        context.BiometricDevices.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(BiometricDevice entity, CancellationToken cancellationToken = default)
    {
        context.BiometricDevices.Remove(entity);
        return Task.CompletedTask;
    }
}
