using SchoolManagement.BLL.DTOs.Academic;

namespace SchoolManagement.BLL.Interfaces;

public interface IClassSubjectAssignmentService
{
    Task<IReadOnlyList<ClassSubjectAssignmentResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ClassSubjectAssignmentResponseDto> GetByClassSectionAsync(Guid classId, Guid sectionId, CancellationToken cancellationToken = default);
    Task<ClassSubjectAssignmentResponseDto> UpsertAsync(UpsertClassSubjectAssignmentDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
