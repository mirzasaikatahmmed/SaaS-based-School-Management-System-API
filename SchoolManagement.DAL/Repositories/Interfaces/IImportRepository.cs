using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IImportRepository
{
    Task<ImportBatch> AddBatchAsync(ImportBatch batch, CancellationToken cancellationToken = default);
    Task UpdateBatchAsync(ImportBatch batch, CancellationToken cancellationToken = default);
    Task<ImportBatch?> GetBatchByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ImportBatch?> GetBatchWithRowsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<ImportBatch> Items, int TotalCount)> GetBatchesAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ImportBatchRow> AddRowAsync(ImportBatchRow row, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ImportBatchRow>> GetFailedRowsAsync(Guid batchId, CancellationToken cancellationToken = default);
}
