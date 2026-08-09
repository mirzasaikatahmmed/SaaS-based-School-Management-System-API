using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IGradeRangeRepository
{
    Task<IReadOnlyList<GradeRange>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<GradeRange?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> OverlapsAsync(decimal minPercentage, decimal maxPercentage, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<int> CountExamPositionsAsync(CancellationToken cancellationToken = default);
    Task<GradeRange> AddAsync(GradeRange entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(GradeRange entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(GradeRange entity, CancellationToken cancellationToken = default);
}
