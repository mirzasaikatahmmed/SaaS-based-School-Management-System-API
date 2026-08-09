using SchoolManagement.BLL.DTOs.Leave;

namespace SchoolManagement.BLL.Interfaces;

public interface ILeaveCategoryService
{
    Task<IReadOnlyList<LeaveCategoryResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LeaveCategoryLookupDto>> GetLookupAsync(string? role, CancellationToken cancellationToken = default);
    Task<LeaveCategoryResponseDto> CreateAsync(CreateLeaveCategoryDto dto, CancellationToken cancellationToken = default);
    Task<LeaveCategoryResponseDto> UpdateAsync(Guid id, UpdateLeaveCategoryDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
