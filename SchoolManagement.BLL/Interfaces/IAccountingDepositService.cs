using SchoolManagement.BLL.DTOs.OfficeAccounting;

namespace SchoolManagement.BLL.Interfaces;

public interface IAccountingDepositService
{
    Task<AccountingDepositListResponseDto> GetFilteredAsync(AccountingDepositFilterDto filter, CancellationToken cancellationToken = default);
    Task<AccountingDepositResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AccountingDepositResponseDto> CreateAsync(CreateAccountingDepositDto dto, CancellationToken cancellationToken = default);
    Task<AccountingDepositResponseDto> UpdateAsync(Guid id, UpdateAccountingDepositDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AccountingDepositResponseDto> UploadAttachmentAsync(Guid id, Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default);
}
