using SchoolManagement.DAL.Entities.Master;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IBiometricDeviceRegistryRepository
{
    Task<BiometricDeviceRegistry?> GetBySerialNumberAsync(string serialNumber, CancellationToken cancellationToken = default);
    Task<bool> SerialNumberExistsAsync(string serialNumber, CancellationToken cancellationToken = default);
    Task<BiometricDeviceRegistry> AddAsync(BiometricDeviceRegistry entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(BiometricDeviceRegistry entity, CancellationToken cancellationToken = default);
    Task DeleteBySerialNumberAsync(string serialNumber, CancellationToken cancellationToken = default);
}
