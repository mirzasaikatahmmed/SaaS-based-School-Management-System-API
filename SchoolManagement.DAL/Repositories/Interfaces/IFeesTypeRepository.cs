using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IFeesTypeRepository
{
    Task<IReadOnlyList<FeesType>> GetAllAsync(bool? isActive = null, CancellationToken cancellationToken = default);
    Task<FeesType?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> FeeCodeExistsAsync(string feeCode, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<int> CountGroupItemsUsingAsync(Guid feesTypeId, CancellationToken cancellationToken = default);
    Task<FeesType> AddAsync(FeesType entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(FeesType entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(FeesType entity, CancellationToken cancellationToken = default);
}
