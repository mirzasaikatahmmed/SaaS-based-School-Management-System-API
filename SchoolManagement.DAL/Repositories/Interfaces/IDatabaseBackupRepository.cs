using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IDatabaseBackupRepository
{
    Task<(IReadOnlyList<DatabaseBackup> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<DatabaseBackup?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DatabaseBackup> AddAsync(DatabaseBackup entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(DatabaseBackup entity, CancellationToken cancellationToken = default);
}
