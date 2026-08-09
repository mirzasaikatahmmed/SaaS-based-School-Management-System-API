using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class BiometricUserMapRepository(TenantDbContext context) : IBiometricUserMapRepository
{
    public async Task<IReadOnlyList<BiometricUserMap>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.BiometricUserMaps
            .Include(m => m.Student)
            .Include(m => m.Employee)
            .OrderBy(m => m.DevicePin)
            .ToListAsync(cancellationToken);

    public async Task<BiometricUserMap?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.BiometricUserMaps
            .Include(m => m.Student)
            .Include(m => m.Employee)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task<BiometricUserMap?> GetByPinAsync(string devicePin, bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var q = context.BiometricUserMaps.Where(m => m.DevicePin == devicePin);
        if (activeOnly) q = q.Where(m => m.IsActive);
        return await q.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> PinExistsAsync(string devicePin, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var q = context.BiometricUserMaps.Where(m => m.DevicePin == devicePin);
        if (excludeId.HasValue) q = q.Where(m => m.Id != excludeId.Value);
        return await q.AnyAsync(cancellationToken);
    }

    public async Task<BiometricUserMap> AddAsync(BiometricUserMap entity, CancellationToken cancellationToken = default)
    {
        await context.BiometricUserMaps.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(BiometricUserMap entity, CancellationToken cancellationToken = default)
    {
        context.BiometricUserMaps.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(BiometricUserMap entity, CancellationToken cancellationToken = default)
    {
        context.BiometricUserMaps.Remove(entity);
        return Task.CompletedTask;
    }
}
