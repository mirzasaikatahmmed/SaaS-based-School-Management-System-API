using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class AccountingAccountRepository(TenantDbContext context) : IAccountingAccountRepository
{
    public async Task<IReadOnlyList<AccountingAccount>> GetAllAsync(bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var q = context.AccountingAccounts.AsQueryable();
        if (isActive.HasValue) q = q.Where(x => x.IsActive == isActive.Value);
        return await q.OrderBy(x => x.AccountName).ToListAsync(cancellationToken);
    }

    public async Task<AccountingAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.AccountingAccounts.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
        => await context.AccountingAccounts.AnyAsync(x => x.AccountName.ToLower() == name.ToLower() && (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);

    public async Task<AccountingAccount> AddAsync(AccountingAccount entity, CancellationToken cancellationToken = default)
    {
        await context.AccountingAccounts.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(AccountingAccount entity, CancellationToken cancellationToken = default)
    {
        context.AccountingAccounts.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(AccountingAccount entity, CancellationToken cancellationToken = default)
    {
        context.AccountingAccounts.Remove(entity);
        return Task.CompletedTask;
    }
}
