using SchoolManagement.BLL.DTOs.Academic;

namespace SchoolManagement.BLL.Interfaces;

public interface IClassControlService
{
    Task<IReadOnlyList<ClassResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ClassResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ClassResponseDto> CreateAsync(CreateClassDto dto, CancellationToken cancellationToken = default);
    Task<ClassResponseDto> UpdateAsync(Guid id, UpdateClassDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
