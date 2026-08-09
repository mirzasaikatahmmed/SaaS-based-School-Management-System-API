using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IBiometricDeviceRepository
{
    Task<IReadOnlyList<BiometricDevice>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<BiometricDevice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BiometricDevice?> GetBySerialNumberAsync(string serialNumber, CancellationToken cancellationToken = default);
    Task<bool> SerialNumberExistsAsync(string serialNumber, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<BiometricDevice> AddAsync(BiometricDevice entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(BiometricDevice entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(BiometricDevice entity, CancellationToken cancellationToken = default);
}
