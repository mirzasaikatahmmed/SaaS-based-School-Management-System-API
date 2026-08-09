using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class BookRepository(TenantDbContext context) : IBookRepository
{
    public async Task<(IReadOnlyList<Book> Items, int TotalCount)> SearchAsync(BookSearchFilter filter, CancellationToken cancellationToken = default)
    {
        var q = context.Books.Include(b => b.Category).AsQueryable();

        if (filter.CategoryId.HasValue)
            q = q.Where(b => b.CategoryId == filter.CategoryId.Value);
        if (filter.IsActive.HasValue)
            q = q.Where(b => b.IsActive == filter.IsActive.Value);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            q = q.Where(b =>
                b.Title.ToLower().Contains(s) ||
                (b.Author != null && b.Author.ToLower().Contains(s)) ||
                (b.IsbnNo != null && b.IsbnNo.ToLower().Contains(s)) ||
                (b.Publisher != null && b.Publisher.ToLower().Contains(s)));
        }

        var total = await q.CountAsync(cancellationToken);
        var page = filter.Page < 1 ? 1 : filter.Page;
        var size = filter.PageSize is < 1 or > 200 ? 25 : filter.PageSize;
        var items = await q.OrderBy(b => b.Title)
            .Skip((page - 1) * size).Take(size)
            .ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Books.Include(b => b.Category).FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public async Task<bool> TitleExistsAsync(string title, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var t = title.Trim().ToUpperInvariant();
        var q = context.Books.Where(b => b.Title.ToUpper() == t);
        if (excludeId.HasValue) q = q.Where(b => b.Id != excludeId.Value);
        return await q.AnyAsync(cancellationToken);
    }

    public async Task<Book> AddAsync(Book entity, CancellationToken cancellationToken = default)
    {
        await context.Books.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(Book entity, CancellationToken cancellationToken = default)
    {
        context.Books.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Book entity, CancellationToken cancellationToken = default)
    {
        context.Books.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<Book>> GetLookupAsync(CancellationToken cancellationToken = default)
        => await context.Books
            .Where(b => b.IsActive)
            .OrderBy(b => b.Title)
            .ToListAsync(cancellationToken);

    public async Task<int> CountActiveIssuesAsync(Guid bookId, CancellationToken cancellationToken = default)
        => await context.BookIssues.CountAsync(
            i => i.BookId == bookId && (i.Status == "Issued" || i.Status == "Overdue"), cancellationToken);
}
