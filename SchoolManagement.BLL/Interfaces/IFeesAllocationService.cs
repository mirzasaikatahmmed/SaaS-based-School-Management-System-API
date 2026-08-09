using SchoolManagement.BLL.DTOs.StudentAccounting;

namespace SchoolManagement.BLL.Interfaces;

public interface IFeesAllocationService
{
    Task<IReadOnlyList<FeesAllocationResponseDto>> GetFilteredAsync(FeesAllocationFilterDto filter, CancellationToken cancellationToken = default);
    Task<FeesAllocationResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<FeesAllocationResponseDto> CreateAsync(CreateFeesAllocationDto dto, CancellationToken cancellationToken = default);
    Task<FeesAllocationResponseDto> UpdateAsync(Guid id, UpdateFeesAllocationDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GenerateInvoicesResultDto> GenerateInvoicesForAllocationAsync(Guid allocationId, CancellationToken cancellationToken = default);
}
