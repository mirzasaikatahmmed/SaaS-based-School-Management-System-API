using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IBiometricUserMapRepository
{
    Task<IReadOnlyList<BiometricUserMap>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<BiometricUserMap?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BiometricUserMap?> GetByPinAsync(string devicePin, bool activeOnly = true, CancellationToken cancellationToken = default);
    Task<bool> PinExistsAsync(string devicePin, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<BiometricUserMap> AddAsync(BiometricUserMap entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(BiometricUserMap entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(BiometricUserMap entity, CancellationToken cancellationToken = default);
}
