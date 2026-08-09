using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IFeesGroupRepository
{
    Task<IReadOnlyList<FeesGroup>> GetAllAsync(bool? isActive = null, CancellationToken cancellationToken = default);
    Task<FeesGroup?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<int> CountAllocationsUsingAsync(Guid groupId, CancellationToken cancellationToken = default);
    Task<FeesGroup> AddAsync(FeesGroup entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(FeesGroup entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(FeesGroup entity, CancellationToken cancellationToken = default);
    Task ReplaceItemsAsync(Guid groupId, IEnumerable<FeesGroupItem> items, CancellationToken cancellationToken = default);
}
