using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IMarkDistributionRepository
{
    Task<IReadOnlyList<MarkDistribution>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<MarkDistribution?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<int> CountExamsUsingAsync(Guid distributionId, CancellationToken cancellationToken = default);
    Task<MarkDistribution> AddAsync(MarkDistribution entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(MarkDistribution entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(MarkDistribution entity, CancellationToken cancellationToken = default);
}
