using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public class BookIssueSearchFilter
{
    public string? Status { get; set; }
    public string? Role { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public interface IBookIssueRepository
{
    Task<(IReadOnlyList<BookIssue> Items, int TotalCount)> SearchAsync(BookIssueSearchFilter filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BookIssue>> GetMyAsync(Guid? studentId, Guid? employeeId, CancellationToken cancellationToken = default);
    Task<BookIssue?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> CountActiveByBorrowerAsync(Guid bookId, Guid? studentId, Guid? employeeId, CancellationToken cancellationToken = default);
    Task<BookIssue> AddAsync(BookIssue entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(BookIssue entity, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BookIssue>> GetIssuedAsync(CancellationToken cancellationToken = default);
}
