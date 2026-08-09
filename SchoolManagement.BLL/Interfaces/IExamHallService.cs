using SchoolManagement.BLL.DTOs.ExamMaster;

namespace SchoolManagement.BLL.Interfaces;

public interface IExamHallService
{
    Task<IReadOnlyList<ExamHallResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ExamHallResponseDto> CreateAsync(CreateExamHallDto dto, CancellationToken cancellationToken = default);
    Task<ExamHallResponseDto> UpdateAsync(Guid id, UpdateExamHallDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExamHallLookupDto>> GetLookupAsync(CancellationToken cancellationToken = default);
}
