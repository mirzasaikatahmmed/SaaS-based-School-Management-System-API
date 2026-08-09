using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IClassTeacherRepository
{
    Task<IReadOnlyList<ClassTeacherAllocation>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ClassTeacherAllocation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ClassTeacherAllocation?> GetByClassSectionAsync(Guid classId, Guid sectionId, CancellationToken cancellationToken = default);
    Task<ClassTeacherAllocation> AddAsync(ClassTeacherAllocation entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(ClassTeacherAllocation entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(ClassTeacherAllocation entity, CancellationToken cancellationToken = default);
}
