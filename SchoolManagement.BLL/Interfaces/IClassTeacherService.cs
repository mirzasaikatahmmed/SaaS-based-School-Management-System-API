using SchoolManagement.BLL.DTOs.Academic;

namespace SchoolManagement.BLL.Interfaces;

public interface IClassTeacherService
{
    Task<IReadOnlyList<ClassTeacherResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ClassTeacherResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ClassTeacherResponseDto> UpsertAsync(UpsertClassTeacherDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
