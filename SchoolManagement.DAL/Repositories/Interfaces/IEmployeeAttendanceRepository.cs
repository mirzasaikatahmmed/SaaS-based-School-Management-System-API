using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IEmployeeAttendanceRepository
{
    Task<IReadOnlyList<EmployeeAttendance>> GetForDateAsync(
        string? role, DateTime date, CancellationToken cancellationToken = default);

    Task UpsertBatchAsync(IEnumerable<EmployeeAttendance> entries, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EmployeeAttendance>> GetReportAsync(
        string? role, Guid? employeeId, DateTime fromDate, DateTime toDate,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Employee>> GetActiveEmployeesByRoleAsync(
        string? role, CancellationToken cancellationToken = default);
}
