using SchoolManagement.BLL.DTOs.Marks;

namespace SchoolManagement.BLL.Interfaces;

public interface IGradeRangeService
{
    Task<IReadOnlyList<GradeRangeDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<GradeRangeDto> CreateAsync(CreateGradeRangeDto dto, CancellationToken cancellationToken = default);
    Task<GradeRangeDto> UpdateAsync(Guid id, UpdateGradeRangeDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
