using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IDeactivateReasonRepository
{
    Task<IReadOnlyList<DeactivateReason>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DeactivateReason?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DeactivateReason?> GetByReasonAsync(string reason, CancellationToken cancellationToken = default);
    Task<bool> ReasonExistsAsync(string reason, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<int> CountStudentsUsingAsync(Guid reasonId, CancellationToken cancellationToken = default);
    Task<DeactivateReason> AddAsync(DeactivateReason entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(DeactivateReason entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(DeactivateReason entity, CancellationToken cancellationToken = default);
}
