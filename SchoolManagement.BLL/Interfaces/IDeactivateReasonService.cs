using SchoolManagement.BLL.DTOs.StudentDetails;

namespace SchoolManagement.BLL.Interfaces;

public interface IDeactivateReasonService
{
    Task<IReadOnlyList<DeactivateReasonDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DeactivateReasonDto> CreateAsync(CreateDeactivateReasonDto dto, CancellationToken cancellationToken = default);
    Task<DeactivateReasonDto> UpdateAsync(Guid id, UpdateDeactivateReasonDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
