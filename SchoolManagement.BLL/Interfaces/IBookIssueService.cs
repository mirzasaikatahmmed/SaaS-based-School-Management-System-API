using SchoolManagement.BLL.DTOs.Library;

namespace SchoolManagement.BLL.Interfaces;

public interface IBookIssueService
{
    Task<BookIssueListResponseDto> GetListAsync(BookIssueFilterDto filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BookIssueListItemDto>> GetMyAsync(CancellationToken cancellationToken = default);
    Task<BookIssueListItemDto> IssueAsync(IssueBookDto dto, CancellationToken cancellationToken = default);
    Task<BookIssueListItemDto> ReturnAsync(Guid id, ReturnBookDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BorrowerLookupDto>> GetBorrowersLookupAsync(string role, CancellationToken cancellationToken = default);
}
