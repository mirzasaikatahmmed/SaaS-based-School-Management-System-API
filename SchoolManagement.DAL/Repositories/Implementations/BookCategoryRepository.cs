using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class BookCategoryRepository(TenantDbContext context) : IBookCategoryRepository
{
    public async Task<IReadOnlyList<BookCategory>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.BookCategories.OrderBy(c => c.Name).ToListAsync(cancellationToken);

    public async Task<BookCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.BookCategories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var n = name.Trim().ToUpperInvariant();
        var q = context.BookCategories.Where(c => c.Name.ToUpper() == n);
        if (excludeId.HasValue) q = q.Where(c => c.Id != excludeId.Value);
        return await q.AnyAsync(cancellationToken);
    }

    public async Task<int> CountBooksUsingAsync(Guid categoryId, CancellationToken cancellationToken = default)
        => await context.Books.CountAsync(b => b.CategoryId == categoryId, cancellationToken);

    public async Task<BookCategory> AddAsync(BookCategory entity, CancellationToken cancellationToken = default)
    {
        await context.BookCategories.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(BookCategory entity, CancellationToken cancellationToken = default)
    {
        context.BookCategories.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(BookCategory entity, CancellationToken cancellationToken = default)
    {
        context.BookCategories.Remove(entity);
        return Task.CompletedTask;
    }
}
