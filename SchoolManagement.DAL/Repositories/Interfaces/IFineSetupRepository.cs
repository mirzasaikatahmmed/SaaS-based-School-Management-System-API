using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IFineSetupRepository
{
    Task<IReadOnlyList<FineSetup>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<FineSetup?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid groupId, Guid feesTypeId, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<FineSetup> AddAsync(FineSetup entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(FineSetup entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(FineSetup entity, CancellationToken cancellationToken = default);
}
