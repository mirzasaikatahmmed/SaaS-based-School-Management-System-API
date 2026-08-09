using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface ILeaveCategoryRepository
{
    Task<IReadOnlyList<LeaveCategory>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LeaveCategory>> GetByRoleAsync(string role, CancellationToken cancellationToken = default);
    Task<LeaveCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> NameRoleExistsAsync(string name, string role, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<int> CountRequestsAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<LeaveCategory> AddAsync(LeaveCategory entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(LeaveCategory entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(LeaveCategory entity, CancellationToken cancellationToken = default);
}
