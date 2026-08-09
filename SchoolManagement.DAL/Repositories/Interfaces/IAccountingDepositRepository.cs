using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public class AccountingTransactionFilter
{
    public Guid? AccountId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public interface IAccountingDepositRepository
{
    Task<(IReadOnlyList<AccountingDeposit> Items, int TotalCount)> GetFilteredAsync(AccountingTransactionFilter filter, CancellationToken cancellationToken = default);
    Task<AccountingDeposit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AccountingDeposit> AddAsync(AccountingDeposit entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(AccountingDeposit entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(AccountingDeposit entity, CancellationToken cancellationToken = default);
}
