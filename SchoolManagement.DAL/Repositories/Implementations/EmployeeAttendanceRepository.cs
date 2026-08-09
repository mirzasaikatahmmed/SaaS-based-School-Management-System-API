using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class EmployeeAttendanceRepository(TenantDbContext context) : IEmployeeAttendanceRepository
{
    public async Task<IReadOnlyList<EmployeeAttendance>> GetForDateAsync(
        string? role, DateTime date, CancellationToken cancellationToken = default)
    {
        var q = context.EmployeeAttendances
            .Include(a => a.Employee)
            .Where(a => a.AttendanceDate.Date == date.Date);
        if (!string.IsNullOrWhiteSpace(role))
            q = q.Where(a => a.Employee.Role.ToLower() == role.Trim().ToLower());
        return await q.ToListAsync(cancellationToken);
    }

    public async Task UpsertBatchAsync(IEnumerable<EmployeeAttendance> entries, CancellationToken cancellationToken = default)
    {
        foreach (var entry in entries)
        {
            var existing = await context.EmployeeAttendances.FirstOrDefaultAsync(a =>
                a.EmployeeId == entry.EmployeeId &&
                a.AttendanceDate.Date == entry.AttendanceDate.Date, cancellationToken);

            if (existing is null)
            {
                await context.EmployeeAttendances.AddAsync(entry, cancellationToken);
                continue;
            }

            existing.Status = entry.Status;
            existing.Remarks = entry.Remarks;
            existing.CreatedBy = entry.CreatedBy ?? existing.CreatedBy;
            existing.UpdatedAt = DateTime.UtcNow;
            context.EmployeeAttendances.Update(existing);
        }
    }

    public async Task<IReadOnlyList<EmployeeAttendance>> GetReportAsync(
        string? role, Guid? employeeId, DateTime fromDate, DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        var q = context.EmployeeAttendances
            .Include(a => a.Employee)
            .Where(a => a.AttendanceDate.Date >= fromDate.Date && a.AttendanceDate.Date <= toDate.Date);

        if (!string.IsNullOrWhiteSpace(role))
            q = q.Where(a => a.Employee.Role.ToLower() == role.Trim().ToLower());
        if (employeeId.HasValue)
            q = q.Where(a => a.EmployeeId == employeeId.Value);

        return await q.OrderBy(a => a.AttendanceDate).ThenBy(a => a.Employee.Name).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Employee>> GetActiveEmployeesByRoleAsync(
        string? role, CancellationToken cancellationToken = default)
    {
        var q = context.Employees.Where(e => e.IsActive);
        if (!string.IsNullOrWhiteSpace(role))
            q = q.Where(e => e.Role.ToLower() == role.Trim().ToLower());
        return await q.OrderBy(e => e.Name).ToListAsync(cancellationToken);
    }
}
