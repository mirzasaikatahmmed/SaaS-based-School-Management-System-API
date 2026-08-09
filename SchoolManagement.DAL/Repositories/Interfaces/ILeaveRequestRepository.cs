using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public class LeaveRequestSearchFilter
{
    public Guid? EmployeeId { get; set; }
    public string? Role { get; set; }
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public interface ILeaveRequestRepository
{
    Task<LeaveRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<LeaveRequest> Items, int TotalCount)> SearchAsync(LeaveRequestSearchFilter filter, CancellationToken cancellationToken = default);
    Task<int> SumUsedDaysAsync(Guid employeeId, Guid categoryId, int year, CancellationToken cancellationToken = default);
    Task<LeaveRequest> AddAsync(LeaveRequest entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(LeaveRequest entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(LeaveRequest entity, CancellationToken cancellationToken = default);
}
