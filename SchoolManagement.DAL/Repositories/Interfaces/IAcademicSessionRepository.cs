using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IAcademicSessionRepository
{
    Task<IReadOnlyList<AcademicSession>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AcademicSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AcademicSession?> GetCurrentAsync(CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task ClearSelectedAsync(CancellationToken cancellationToken = default);
    Task<AcademicSession> AddAsync(AcademicSession entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(AcademicSession entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(AcademicSession entity, CancellationToken cancellationToken = default);
    Task<int> CountStudentsForYearAsync(int academicYear, CancellationToken cancellationToken = default);
}
