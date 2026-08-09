using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IBiometricPunchLogRepository
{
    Task<(IReadOnlyList<BiometricPunchLog> Items, int TotalCount)> GetFilteredAsync(
        DateTime? from, DateTime? to, Guid? deviceId, string? kind,
        int page, int pageSize, CancellationToken cancellationToken = default);

    Task<BiometricPunchLog> AddAsync(BiometricPunchLog entity, CancellationToken cancellationToken = default);
}
