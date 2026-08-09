using SchoolManagement.BLL.DTOs.StudentAccounting;

namespace SchoolManagement.BLL.Interfaces;

public interface IOfflinePaymentService
{
    Task<OfflinePaymentResponseDto> SubmitAsync(CreateOfflinePaymentDto dto, CancellationToken cancellationToken = default);
    Task<OfflinePaymentListResponseDto> GetFilteredAsync(OfflinePaymentFilterDto filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OfflinePaymentResponseDto>> GetMyPaymentsAsync(CancellationToken cancellationToken = default);
    Task<OfflinePaymentResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OfflinePaymentResponseDto> ApproveAsync(Guid id, ReviewOfflinePaymentDto dto, CancellationToken cancellationToken = default);
    Task<OfflinePaymentResponseDto> RejectAsync(Guid id, ReviewOfflinePaymentDto dto, CancellationToken cancellationToken = default);
}
