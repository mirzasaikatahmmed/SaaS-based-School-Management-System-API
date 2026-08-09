using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class AdvanceSalaryRepository(TenantDbContext context) : IAdvanceSalaryRepository
{
    public async Task<AdvanceSalaryRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.AdvanceSalaryRequests
            .Include(x => x.Employee).ThenInclude(e => e.Designation)
            .Include(x => x.Employee).ThenInclude(e => e.Department)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<AdvanceSalaryRequest> Items, int TotalCount)> SearchAsync(
        AdvanceSalarySearchFilter filter, CancellationToken cancellationToken = default)
    {
        var q = context.AdvanceSalaryRequests
            .Include(x => x.Employee).ThenInclude(e => e.Designation)
            .Include(x => x.Employee).ThenInclude(e => e.Department)
            .AsQueryable();

        if (filter.EmployeeId.HasValue)
            q = q.Where(x => x.EmployeeId == filter.EmployeeId.Value);
        if (!string.IsNullOrWhiteSpace(filter.DeductMonth))
            q = q.Where(x => x.DeductMonth == filter.DeductMonth);
        if (!string.IsNullOrWhiteSpace(filter.Status))
            q = q.Where(x => x.Status == filter.Status);
        if (!string.IsNullOrWhiteSpace(filter.Role))
            q = q.Where(x => x.Employee.Role.ToLower() == filter.Role.Trim().ToLower());
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            q = q.Where(x =>
                x.Employee.Name.ToLower().Contains(s) ||
                x.Employee.StaffId.ToLower().Contains(s) ||
                (x.Reason != null && x.Reason.ToLower().Contains(s)));
        }

        var total = await q.CountAsync(cancellationToken);
        var page = filter.Page < 1 ? 1 : filter.Page;
        var size = filter.PageSize is < 1 or > 200 ? 25 : filter.PageSize;
        var items = await q.OrderByDescending(x => x.AppliedOn)
            .Skip((page - 1) * size).Take(size)
            .ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task<bool> HasPendingForMonthAsync(Guid employeeId, string deductMonth, CancellationToken cancellationToken = default)
        => await context.AdvanceSalaryRequests.AnyAsync(
            x => x.EmployeeId == employeeId &&
                 x.DeductMonth == deductMonth &&
                 x.Status == HrRequestStatuses.Pending,
            cancellationToken);

    public async Task<decimal> SumApprovedForMonthAsync(Guid employeeId, string deductMonth, CancellationToken cancellationToken = default)
        => await context.AdvanceSalaryRequests
            .Where(x => x.EmployeeId == employeeId &&
                        x.DeductMonth == deductMonth &&
                        x.Status == HrRequestStatuses.Approved)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0;

    public async Task<AdvanceSalaryRequest> AddAsync(AdvanceSalaryRequest entity, CancellationToken cancellationToken = default)
    {
        await context.AdvanceSalaryRequests.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(AdvanceSalaryRequest entity, CancellationToken cancellationToken = default)
    {
        context.AdvanceSalaryRequests.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(AdvanceSalaryRequest entity, CancellationToken cancellationToken = default)
    {
        context.AdvanceSalaryRequests.Remove(entity);
        return Task.CompletedTask;
    }
}
