using SchoolManagement.BLL.DTOs.StudentAccounting;

namespace SchoolManagement.BLL.Interfaces;

public interface IFeesTypeService
{
    Task<IReadOnlyList<FeesTypeResponseDto>> GetAllAsync(bool? isActive, CancellationToken cancellationToken = default);
    Task<FeesTypeResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<FeesTypeResponseDto> CreateAsync(CreateFeesTypeDto dto, CancellationToken cancellationToken = default);
    Task<FeesTypeResponseDto> UpdateAsync(Guid id, UpdateFeesTypeDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FeesTypeLookupDto>> GetLookupAsync(CancellationToken cancellationToken = default);
}
