using SchoolManagement.BLL.DTOs.Library;

namespace SchoolManagement.BLL.Interfaces;

public interface IBookCategoryService
{
    Task<IReadOnlyList<BookCategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<BookCategoryDto> CreateAsync(CreateBookCategoryDto dto, CancellationToken cancellationToken = default);
    Task<BookCategoryDto> UpdateAsync(Guid id, UpdateBookCategoryDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
