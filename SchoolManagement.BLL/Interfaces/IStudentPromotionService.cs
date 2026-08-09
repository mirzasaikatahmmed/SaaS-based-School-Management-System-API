using SchoolManagement.BLL.DTOs.Academic;

namespace SchoolManagement.BLL.Interfaces;

public interface IStudentPromotionService
{
    Task<PromotionStudentListResponseDto> GetStudentsAsync(PromotionFilterDto filter, CancellationToken cancellationToken = default);
    Task<ProcessPromotionResultDto> ProcessAsync(ProcessPromotionDto dto, CancellationToken cancellationToken = default);
}
