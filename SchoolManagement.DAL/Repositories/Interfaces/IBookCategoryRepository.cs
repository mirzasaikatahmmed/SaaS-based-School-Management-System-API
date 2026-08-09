using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IBookCategoryRepository
{
    Task<IReadOnlyList<BookCategory>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<BookCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<int> CountBooksUsingAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<BookCategory> AddAsync(BookCategory entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(BookCategory entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(BookCategory entity, CancellationToken cancellationToken = default);
}
