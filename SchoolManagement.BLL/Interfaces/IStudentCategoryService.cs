using SchoolManagement.BLL.DTOs.StudentCategory;

namespace SchoolManagement.BLL.Interfaces;

public interface IStudentCategoryService
{
    Task<IReadOnlyList<StudentCategoryResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<StudentCategoryResponseDto> CreateAsync(CreateStudentCategoryDto dto, CancellationToken cancellationToken = default);
    Task<StudentCategoryResponseDto> UpdateAsync(Guid id, UpdateStudentCategoryDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
