using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class DatabaseBackupRepository(TenantDbContext context) : IDatabaseBackupRepository
{
    public async Task<(IReadOnlyList<DatabaseBackup> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var q = context.DatabaseBackups.AsQueryable();
        var total = await q.CountAsync(cancellationToken);
        var items = await q.OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task<DatabaseBackup?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.DatabaseBackups.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public async Task<DatabaseBackup> AddAsync(DatabaseBackup entity, CancellationToken cancellationToken = default)
    {
        await context.DatabaseBackups.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task DeleteAsync(DatabaseBackup entity, CancellationToken cancellationToken = default)
    {
        context.DatabaseBackups.Remove(entity);
        return Task.CompletedTask;
    }
}
