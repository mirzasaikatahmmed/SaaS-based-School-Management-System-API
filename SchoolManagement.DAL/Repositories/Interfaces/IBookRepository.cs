using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public class BookSearchFilter
{
    public Guid? CategoryId { get; set; }
    public string? Search { get; set; }
    public bool? IsActive { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public interface IBookRepository
{
    Task<(IReadOnlyList<Book> Items, int TotalCount)> SearchAsync(BookSearchFilter filter, CancellationToken cancellationToken = default);
    Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> TitleExistsAsync(string title, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<Book> AddAsync(Book entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Book entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Book entity, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Book>> GetLookupAsync(CancellationToken cancellationToken = default);
    Task<int> CountActiveIssuesAsync(Guid bookId, CancellationToken cancellationToken = default);
}
