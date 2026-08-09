using SchoolManagement.BLL.DTOs.OfficeAccounting;

namespace SchoolManagement.BLL.Interfaces;

public interface IVoucherHeadService
{
    Task<IReadOnlyList<VoucherHeadResponseDto>> GetAllAsync(string? type, CancellationToken cancellationToken = default);
    Task<VoucherHeadResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<VoucherHeadResponseDto> CreateAsync(CreateVoucherHeadDto dto, CancellationToken cancellationToken = default);
    Task<VoucherHeadResponseDto> UpdateAsync(Guid id, UpdateVoucherHeadDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
