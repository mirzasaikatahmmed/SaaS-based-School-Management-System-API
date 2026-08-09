using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class BookIssueRepository(TenantDbContext context) : IBookIssueRepository
{
    public async Task<(IReadOnlyList<BookIssue> Items, int TotalCount)> SearchAsync(BookIssueSearchFilter filter, CancellationToken cancellationToken = default)
    {
        var q = context.BookIssues
            .Include(i => i.Book)
            .Include(i => i.Student)
            .Include(i => i.Employee)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Status))
            q = q.Where(i => i.Status.ToLower() == filter.Status.Trim().ToLower());
        if (!string.IsNullOrWhiteSpace(filter.Role))
            q = q.Where(i => i.Role.ToLower() == filter.Role.Trim().ToLower());
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            q = q.Where(i =>
                i.Book.Title.ToLower().Contains(s) ||
                (i.UserName != null && i.UserName.ToLower().Contains(s)));
        }

        var total = await q.CountAsync(cancellationToken);
        var page = filter.Page < 1 ? 1 : filter.Page;
        var size = filter.PageSize is < 1 or > 200 ? 25 : filter.PageSize;
        var items = await q.OrderByDescending(i => i.DateOfIssue).ThenByDescending(i => i.CreatedAt)
            .Skip((page - 1) * size).Take(size)
            .ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task<IReadOnlyList<BookIssue>> GetMyAsync(Guid? studentId, Guid? employeeId, CancellationToken cancellationToken = default)
    {
        var q = context.BookIssues.Include(i => i.Book).AsQueryable();
        if (studentId.HasValue)
            q = q.Where(i => i.StudentId == studentId.Value);
        else if (employeeId.HasValue)
            q = q.Where(i => i.EmployeeId == employeeId.Value);
        else
            return [];

        return await q.OrderByDescending(i => i.DateOfIssue).ToListAsync(cancellationToken);
    }

    public async Task<BookIssue?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.BookIssues
            .Include(i => i.Book)
            .Include(i => i.Student)
            .Include(i => i.Employee)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public async Task<int> CountActiveByBorrowerAsync(Guid bookId, Guid? studentId, Guid? employeeId, CancellationToken cancellationToken = default)
    {
        var q = context.BookIssues.Where(i =>
            i.BookId == bookId && (i.Status == "Issued" || i.Status == "Overdue"));
        if (studentId.HasValue) q = q.Where(i => i.StudentId == studentId.Value);
        if (employeeId.HasValue) q = q.Where(i => i.EmployeeId == employeeId.Value);
        return await q.CountAsync(cancellationToken);
    }

    public async Task<BookIssue> AddAsync(BookIssue entity, CancellationToken cancellationToken = default)
    {
        await context.BookIssues.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(BookIssue entity, CancellationToken cancellationToken = default)
    {
        context.BookIssues.Update(entity);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<BookIssue>> GetIssuedAsync(CancellationToken cancellationToken = default)
        => await context.BookIssues
            .Where(i => i.Status == "Issued" || i.Status == "Overdue")
            .ToListAsync(cancellationToken);
}
