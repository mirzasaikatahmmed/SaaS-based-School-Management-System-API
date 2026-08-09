using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IAccountingExpenseRepository
{
    Task<(IReadOnlyList<AccountingExpense> Items, int TotalCount)> GetFilteredAsync(AccountingTransactionFilter filter, CancellationToken cancellationToken = default);
    Task<AccountingExpense?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AccountingExpense> AddAsync(AccountingExpense entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(AccountingExpense entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(AccountingExpense entity, CancellationToken cancellationToken = default);
}
