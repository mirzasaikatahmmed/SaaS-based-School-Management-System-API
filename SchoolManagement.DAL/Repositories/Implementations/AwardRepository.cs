using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class AwardRepository(TenantDbContext context) : IAwardRepository
{
    public async Task<Award?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Awards
            .Include(a => a.Employee)
            .Include(a => a.Student)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<Award> Items, int TotalCount)> SearchAsync(
        AwardSearchFilter filter, CancellationToken cancellationToken = default)
    {
        var q = context.Awards
            .Include(a => a.Employee)
            .Include(a => a.Student)
            .AsQueryable();

        if (filter.EmployeeId.HasValue)
            q = q.Where(a => a.EmployeeId == filter.EmployeeId.Value);
        if (filter.StudentId.HasValue)
            q = q.Where(a => a.StudentId == filter.StudentId.Value);
        if (!string.IsNullOrWhiteSpace(filter.Role))
            q = q.Where(a => a.Role.ToLower() == filter.Role.Trim().ToLower());
        if (filter.FromDate.HasValue)
            q = q.Where(a => a.GivenDate >= filter.FromDate.Value.Date);
        if (filter.ToDate.HasValue)
            q = q.Where(a => a.GivenDate <= filter.ToDate.Value.Date);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            q = q.Where(a =>
                a.AwardName.ToLower().Contains(s) ||
                a.GiftItem.ToLower().Contains(s) ||
                a.AwardReason.ToLower().Contains(s) ||
                (a.Employee != null && a.Employee.Name.ToLower().Contains(s)) ||
                (a.Student != null && (
                    a.Student.FirstName.ToLower().Contains(s) ||
                    (a.Student.LastName != null && a.Student.LastName.ToLower().Contains(s)) ||
                    a.Student.RegisterNo.ToLower().Contains(s))));
        }

        var total = await q.CountAsync(cancellationToken);
        var page = filter.Page < 1 ? 1 : filter.Page;
        var size = filter.PageSize is < 1 or > 200 ? 25 : filter.PageSize;
        var items = await q.OrderByDescending(a => a.GivenDate).ThenByDescending(a => a.CreatedAt)
            .Skip((page - 1) * size).Take(size)
            .ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task<Award> AddAsync(Award entity, CancellationToken cancellationToken = default)
    {
        await context.Awards.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(Award entity, CancellationToken cancellationToken = default)
    {
        context.Awards.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Award entity, CancellationToken cancellationToken = default)
    {
        context.Awards.Remove(entity);
        return Task.CompletedTask;
    }
}
