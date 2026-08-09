using SchoolManagement.BLL.DTOs.Library;

namespace SchoolManagement.BLL.Interfaces;

public interface IBookService
{
    Task<BookListResponseDto> GetListAsync(BookFilterDto filter, CancellationToken cancellationToken = default);
    Task<BookDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BookDetailDto> CreateAsync(CreateBookDto dto, CancellationToken cancellationToken = default);
    Task<BookDetailDto> UpdateAsync(Guid id, UpdateBookDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BookDetailDto> UploadCoverAsync(Guid id, Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BookLookupDto>> GetLookupAsync(CancellationToken cancellationToken = default);
}
