using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public class AdvanceSalarySearchFilter
{
    public Guid? EmployeeId { get; set; }
    public string? DeductMonth { get; set; }
    public string? Status { get; set; }
    public string? Search { get; set; }
    public string? Role { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public interface IAdvanceSalaryRepository
{
    Task<AdvanceSalaryRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<AdvanceSalaryRequest> Items, int TotalCount)> SearchAsync(AdvanceSalarySearchFilter filter, CancellationToken cancellationToken = default);
    Task<bool> HasPendingForMonthAsync(Guid employeeId, string deductMonth, CancellationToken cancellationToken = default);
    Task<decimal> SumApprovedForMonthAsync(Guid employeeId, string deductMonth, CancellationToken cancellationToken = default);
    Task<AdvanceSalaryRequest> AddAsync(AdvanceSalaryRequest entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(AdvanceSalaryRequest entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(AdvanceSalaryRequest entity, CancellationToken cancellationToken = default);
}
