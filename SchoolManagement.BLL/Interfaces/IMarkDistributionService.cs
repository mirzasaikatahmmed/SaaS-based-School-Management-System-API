using SchoolManagement.BLL.DTOs.ExamMaster;

namespace SchoolManagement.BLL.Interfaces;

public interface IMarkDistributionService
{
    Task<IReadOnlyList<MarkDistributionResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<MarkDistributionResponseDto> CreateAsync(CreateMarkDistributionDto dto, CancellationToken cancellationToken = default);
    Task<MarkDistributionResponseDto> UpdateAsync(Guid id, UpdateMarkDistributionDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
