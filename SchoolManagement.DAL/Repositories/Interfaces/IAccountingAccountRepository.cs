using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IAccountingAccountRepository
{
    Task<IReadOnlyList<AccountingAccount>> GetAllAsync(bool? isActive = null, CancellationToken cancellationToken = default);
    Task<AccountingAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<AccountingAccount> AddAsync(AccountingAccount entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(AccountingAccount entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(AccountingAccount entity, CancellationToken cancellationToken = default);
}
