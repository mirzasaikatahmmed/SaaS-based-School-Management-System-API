using SchoolManagement.BLL.DTOs.StudentAccounting;

namespace SchoolManagement.BLL.Interfaces;

public interface IFeesGroupService
{
    Task<IReadOnlyList<FeesGroupResponseDto>> GetAllAsync(bool? isActive, CancellationToken cancellationToken = default);
    Task<FeesGroupResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<FeesGroupResponseDto> CreateAsync(CreateFeesGroupDto dto, CancellationToken cancellationToken = default);
    Task<FeesGroupResponseDto> UpdateAsync(Guid id, UpdateFeesGroupDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FeesGroupLookupDto>> GetLookupAsync(CancellationToken cancellationToken = default);
}
