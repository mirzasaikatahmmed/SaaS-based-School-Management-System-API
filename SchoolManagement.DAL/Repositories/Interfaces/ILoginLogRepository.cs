using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface ILoginLogRepository
{
    Task<(IReadOnlyList<LoginLog> Items, int Total)> SearchAsync(
        string? type,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task ClearAsync(CancellationToken cancellationToken = default);
}
