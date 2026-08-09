using SchoolManagement.BLL.DTOs.Academic;

namespace SchoolManagement.BLL.Interfaces;

public interface ISubjectService
{
    Task<IReadOnlyList<SubjectResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SubjectResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SubjectResponseDto> CreateAsync(CreateSubjectDto dto, CancellationToken cancellationToken = default);
    Task<SubjectResponseDto> UpdateAsync(Guid id, UpdateSubjectDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
