using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IStudentCategoryRepository
{
    Task<IReadOnlyList<StudentCategory>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<StudentCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<int> CountStudentsUsingAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<StudentCategory> AddAsync(StudentCategory category, CancellationToken cancellationToken = default);
    Task UpdateAsync(StudentCategory category, CancellationToken cancellationToken = default);
    Task DeleteAsync(StudentCategory category, CancellationToken cancellationToken = default);
}
