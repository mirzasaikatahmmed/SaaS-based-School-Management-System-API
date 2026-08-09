using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface ISubjectRepository
{
    Task<IReadOnlyList<Subject>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Subject?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<int> CountAssignmentUsageAsync(Guid subjectId, CancellationToken cancellationToken = default);
    Task<Subject> AddAsync(Subject entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Subject entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Subject entity, CancellationToken cancellationToken = default);
}
