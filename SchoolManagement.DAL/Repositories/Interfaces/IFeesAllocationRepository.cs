using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public class FeesAllocationFilter
{
    public Guid? ClassId { get; set; }
    public Guid? SectionId { get; set; }
    public int? AcademicYear { get; set; }
    public bool? IsActive { get; set; }
}

public interface IFeesAllocationRepository
{
    Task<IReadOnlyList<FeesAllocation>> GetFilteredAsync(FeesAllocationFilter filter, CancellationToken cancellationToken = default);
    Task<FeesAllocation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsUniqueAsync(Guid classId, Guid sectionId, Guid feesGroupId, int academicYear, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<FeesAllocation> AddAsync(FeesAllocation entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(FeesAllocation entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(FeesAllocation entity, CancellationToken cancellationToken = default);
}
