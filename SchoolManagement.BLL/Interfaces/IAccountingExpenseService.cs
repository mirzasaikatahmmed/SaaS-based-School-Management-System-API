using SchoolManagement.BLL.DTOs.OfficeAccounting;

namespace SchoolManagement.BLL.Interfaces;

public interface IAccountingExpenseService
{
    Task<AccountingExpenseListResponseDto> GetFilteredAsync(AccountingExpenseFilterDto filter, CancellationToken cancellationToken = default);
    Task<AccountingExpenseResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AccountingExpenseResponseDto> CreateAsync(CreateAccountingExpenseDto dto, CancellationToken cancellationToken = default);
    Task<AccountingExpenseResponseDto> UpdateAsync(Guid id, UpdateAccountingExpenseDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AccountingExpenseResponseDto> UploadAttachmentAsync(Guid id, Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default);
}
