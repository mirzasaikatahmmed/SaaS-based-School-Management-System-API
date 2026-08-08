using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IGuardianRepository
{
    Task<Guardian?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Guardian?> GetByIdWithStudentAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Guardian>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task<Guardian> AddAsync(Guardian guardian, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guardian guardian, CancellationToken cancellationToken = default);
}
