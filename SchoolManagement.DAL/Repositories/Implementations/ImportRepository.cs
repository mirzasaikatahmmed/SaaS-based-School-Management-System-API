using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class ImportRepository : IImportRepository
{
    private readonly TenantDbContext _context;

    public ImportRepository(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<ImportBatch> AddBatchAsync(ImportBatch batch, CancellationToken cancellationToken = default)
    {
        await _context.ImportBatches.AddAsync(batch, cancellationToken);
        return batch;
    }

    public Task UpdateBatchAsync(ImportBatch batch, CancellationToken cancellationToken = default)
    {
        _context.ImportBatches.Update(batch);
        return Task.CompletedTask;
    }

    public async Task<ImportBatch?> GetBatchByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ImportBatches
            .Include(b => b.Class)
            .Include(b => b.Section)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<ImportBatch?> GetBatchWithRowsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ImportBatches
            .Include(b => b.Class)
            .Include(b => b.Section)
            .Include(b => b.Rows.OrderBy(r => r.RowNumber))
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<ImportBatch> Items, int TotalCount)> GetBatchesAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ImportBatches
            .Include(b => b.Class)
            .Include(b => b.Section)
            .AsQueryable();

        var total = await query.CountAsync(cancellationToken);
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 200 ? 20 : pageSize;

        var items = await query
            .OrderByDescending(b => b.StartedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<ImportBatchRow> AddRowAsync(ImportBatchRow row, CancellationToken cancellationToken = default)
    {
        await _context.ImportBatchRows.AddAsync(row, cancellationToken);
        return row;
    }

    public async Task<IReadOnlyList<ImportBatchRow>> GetFailedRowsAsync(Guid batchId, CancellationToken cancellationToken = default)
    {
        return await _context.ImportBatchRows
            .Where(r => r.BatchId == batchId && r.Status == ImportBatchRowStatuses.Failed)
            .OrderBy(r => r.RowNumber)
            .ToListAsync(cancellationToken);
    }
}
