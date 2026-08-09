using SchoolManagement.BLL.DTOs.ExamMaster;

namespace SchoolManagement.BLL.Interfaces;

public interface IExamScheduleService
{
    Task<IReadOnlyList<ExamScheduleResponseDto>> GetFilteredAsync(ExamScheduleFilterDto filter, CancellationToken cancellationToken = default);
    Task<ExamScheduleDetailDto> GetDetailAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ExamScheduleDetailDto> CreateAsync(CreateExamScheduleDto dto, CancellationToken cancellationToken = default);
    Task<ExamScheduleDetailDto> UpdateAsync(Guid id, CreateExamScheduleDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
