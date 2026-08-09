using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IFeesGroupItemRepository
{
    Task<IReadOnlyList<FeesGroupItem>> GetByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = default);
    Task<FeesGroupItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<FeesGroupItem> AddAsync(FeesGroupItem entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(FeesGroupItem entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(FeesGroupItem entity, CancellationToken cancellationToken = default);
}
