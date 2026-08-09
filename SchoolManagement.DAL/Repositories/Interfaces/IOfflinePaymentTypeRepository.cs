using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IOfflinePaymentTypeRepository
{
    Task<IReadOnlyList<OfflinePaymentType>> GetAllAsync(bool? isActive = null, CancellationToken cancellationToken = default);
    Task<OfflinePaymentType?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<OfflinePaymentType> AddAsync(OfflinePaymentType entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(OfflinePaymentType entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(OfflinePaymentType entity, CancellationToken cancellationToken = default);
}
