using SchoolManagement.BLL.DTOs.StudentAccounting;

namespace SchoolManagement.BLL.Interfaces;

public interface IOfflinePaymentTypeService
{
    Task<IReadOnlyList<OfflinePaymentTypeResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<OfflinePaymentTypeResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OfflinePaymentTypeResponseDto> CreateAsync(CreateOfflinePaymentTypeDto dto, CancellationToken cancellationToken = default);
    Task<OfflinePaymentTypeResponseDto> UpdateAsync(Guid id, UpdateOfflinePaymentTypeDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
