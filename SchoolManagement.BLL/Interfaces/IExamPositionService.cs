using SchoolManagement.BLL.DTOs.Marks;

namespace SchoolManagement.BLL.Interfaces;

public interface IExamPositionService
{
    Task<IReadOnlyList<ExamPositionItemDto>> GetAsync(ExamPositionFilterDto filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExamPositionItemDto>> GenerateAsync(ExamPositionFilterDto filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExamPositionItemDto>> SaveAsync(SaveExamPositionDto dto, CancellationToken cancellationToken = default);
}
