using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public class AwardSearchFilter
{
    public Guid? EmployeeId { get; set; }
    public Guid? StudentId { get; set; }
    public string? Role { get; set; }
    public string? Search { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public interface IAwardRepository
{
    Task<Award?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Award> Items, int TotalCount)> SearchAsync(AwardSearchFilter filter, CancellationToken cancellationToken = default);
    Task<Award> AddAsync(Award entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Award entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Award entity, CancellationToken cancellationToken = default);
}
