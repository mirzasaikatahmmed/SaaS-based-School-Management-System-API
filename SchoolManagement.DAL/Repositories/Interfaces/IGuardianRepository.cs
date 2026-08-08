using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public class GuardianSearchFilter
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; } = true;
    public bool? IsLoginActive { get; set; }
    public string? SortBy { get; set; }
    public string? SortDir { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public interface IGuardianRepository
{
    Task<Guardian?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Guardian?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Guardian?> GetByIdWithStudentAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Guardian?> GetPrimaryByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Guardian>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Guardian>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Guardian> Items, int TotalCount)> SearchAsync(GuardianSearchFilter filter, CancellationToken cancellationToken = default);
    Task<string> GenerateNextReferenceNoAsync(CancellationToken cancellationToken = default);
    Task BackfillMissingReferenceNosAsync(CancellationToken cancellationToken = default);
    Task<int> CountActiveGuardiansForStudentAsync(Guid studentId, Guid? excludeGuardianId = null, CancellationToken cancellationToken = default);
    Task<Guardian> AddAsync(Guardian guardian, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guardian guardian, CancellationToken cancellationToken = default);
}
