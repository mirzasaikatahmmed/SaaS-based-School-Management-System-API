using SchoolManagement.BLL.DTOs.ExamMaster;

namespace SchoolManagement.BLL.Interfaces;

public interface IExamTermService
{
    Task<IReadOnlyList<ExamTermResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ExamTermResponseDto> CreateAsync(CreateExamTermDto dto, CancellationToken cancellationToken = default);
    Task<ExamTermResponseDto> UpdateAsync(Guid id, UpdateExamTermDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
