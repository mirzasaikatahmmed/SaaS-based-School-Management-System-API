using SchoolManagement.BLL.DTOs.StudentAccounting;

namespace SchoolManagement.BLL.Interfaces;

public interface IFineSetupService
{
    Task<IReadOnlyList<FineSetupResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<FineSetupResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<FineSetupResponseDto> CreateAsync(CreateFineSetupDto dto, CancellationToken cancellationToken = default);
    Task<FineSetupResponseDto> UpdateAsync(Guid id, UpdateFineSetupDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
