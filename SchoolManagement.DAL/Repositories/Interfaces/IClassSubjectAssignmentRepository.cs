using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IClassSubjectAssignmentRepository
{
    Task<IReadOnlyList<ClassSubjectAssignment>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ClassSubjectAssignment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ClassSubjectAssignment?> GetByClassSectionAsync(Guid classId, Guid sectionId, CancellationToken cancellationToken = default);
    Task<ClassSubjectAssignment> AddAsync(ClassSubjectAssignment entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(ClassSubjectAssignment entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(ClassSubjectAssignment entity, CancellationToken cancellationToken = default);
    Task ReplaceItemsAsync(Guid assignmentId, IEnumerable<ClassSubjectAssignmentItem> items, CancellationToken cancellationToken = default);
}
