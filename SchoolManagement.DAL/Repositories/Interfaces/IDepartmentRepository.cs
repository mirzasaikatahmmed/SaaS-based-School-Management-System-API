using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IDepartmentRepository
{
    Task<IReadOnlyList<Department>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Department?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<int> CountEmployeesUsingAsync(Guid departmentId, CancellationToken cancellationToken = default);
    Task<Department> AddAsync(Department entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Department entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Department entity, CancellationToken cancellationToken = default);
}
