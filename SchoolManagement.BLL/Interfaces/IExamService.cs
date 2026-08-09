using SchoolManagement.BLL.DTOs.ExamMaster;

namespace SchoolManagement.BLL.Interfaces;

public interface IExamService
{
    Task<IReadOnlyList<ExamListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ExamResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ExamResponseDto> CreateAsync(CreateExamDto dto, CancellationToken cancellationToken = default);
    Task<ExamResponseDto> UpdateAsync(Guid id, UpdateExamDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ExamResponseDto> TogglePublishAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ExamResponseDto> TogglePublishResultAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExamLookupDto>> GetLookupAsync(CancellationToken cancellationToken = default);
}
