using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IDesignationRepository
{
    Task<IReadOnlyList<Designation>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Designation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Designation?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<int> CountEmployeesUsingAsync(Guid designationId, CancellationToken cancellationToken = default);
    Task<Designation> AddAsync(Designation entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Designation entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Designation entity, CancellationToken cancellationToken = default);
}
