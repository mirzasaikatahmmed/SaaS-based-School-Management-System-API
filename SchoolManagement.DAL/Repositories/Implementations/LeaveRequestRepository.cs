using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class LeaveRequestRepository(TenantDbContext context) : ILeaveRequestRepository
{
    public async Task<LeaveRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.LeaveRequests
            .Include(x => x.Employee)
            .Include(x => x.LeaveCategory)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<LeaveRequest> Items, int TotalCount)> SearchAsync(
        LeaveRequestSearchFilter filter, CancellationToken cancellationToken = default)
    {
        var q = context.LeaveRequests
            .Include(x => x.Employee)
            .Include(x => x.LeaveCategory)
            .AsQueryable();

        if (filter.EmployeeId.HasValue)
            q = q.Where(x => x.EmployeeId == filter.EmployeeId.Value);
        if (!string.IsNullOrWhiteSpace(filter.Role))
            q = q.Where(x => x.Employee.Role.ToLower() == filter.Role.Trim().ToLower());
        if (!string.IsNullOrWhiteSpace(filter.Status))
            q = q.Where(x => x.Status == filter.Status);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            q = q.Where(x =>
                x.Employee.Name.ToLower().Contains(s) ||
                x.LeaveCategory.Name.ToLower().Contains(s) ||
                (x.Reason != null && x.Reason.ToLower().Contains(s)));
        }

        var total = await q.CountAsync(cancellationToken);
        var page = filter.Page < 1 ? 1 : filter.Page;
        var size = filter.PageSize is < 1 or > 200 ? 25 : filter.PageSize;
        var items = await q.OrderByDescending(x => x.ApplyDate)
            .Skip((page - 1) * size).Take(size)
            .ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task<int> SumUsedDaysAsync(Guid employeeId, Guid categoryId, int year, CancellationToken cancellationToken = default)
    {
        var start = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(year + 1, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return await context.LeaveRequests
            .Where(x => x.EmployeeId == employeeId &&
                        x.LeaveCategoryId == categoryId &&
                        (x.Status == HrRequestStatuses.Pending || x.Status == HrRequestStatuses.Approved) &&
                        x.DateOfStart >= start &&
                        x.DateOfStart < end)
            .SumAsync(x => (int?)x.Days, cancellationToken) ?? 0;
    }

    public async Task<LeaveRequest> AddAsync(LeaveRequest entity, CancellationToken cancellationToken = default)
    {
        await context.LeaveRequests.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(LeaveRequest entity, CancellationToken cancellationToken = default)
    {
        context.LeaveRequests.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(LeaveRequest entity, CancellationToken cancellationToken = default)
    {
        context.LeaveRequests.Remove(entity);
        return Task.CompletedTask;
    }
}
