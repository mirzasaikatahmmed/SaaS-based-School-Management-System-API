using SchoolManagement.BLL.DTOs.OfficeAccounting;

namespace SchoolManagement.BLL.Interfaces;

public interface IAccountingAccountService
{
    Task<IReadOnlyList<AccountingAccountResponseDto>> GetAllAsync(bool? isActive, CancellationToken cancellationToken = default);
    Task<AccountingAccountResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AccountingAccountResponseDto> CreateAsync(CreateAccountingAccountDto dto, CancellationToken cancellationToken = default);
    Task<AccountingAccountResponseDto> UpdateAsync(Guid id, UpdateAccountingAccountDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AccountingAccountLookupDto>> GetLookupAsync(CancellationToken cancellationToken = default);
    Task<TransactionListResponseDto> GetTransactionsAsync(TransactionFilterDto filter, CancellationToken cancellationToken = default);
}
