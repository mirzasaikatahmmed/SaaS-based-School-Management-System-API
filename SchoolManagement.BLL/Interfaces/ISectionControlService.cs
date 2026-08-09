using SchoolManagement.BLL.DTOs.Academic;

namespace SchoolManagement.BLL.Interfaces;

public interface ISectionControlService
{
    Task<IReadOnlyList<SectionResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SectionResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SectionResponseDto> CreateAsync(CreateSectionDto dto, CancellationToken cancellationToken = default);
    Task<SectionResponseDto> UpdateAsync(Guid id, UpdateSectionDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
