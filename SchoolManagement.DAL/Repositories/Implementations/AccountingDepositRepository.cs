using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class AccountingDepositRepository(TenantDbContext context) : IAccountingDepositRepository
{
    public async Task<(IReadOnlyList<AccountingDeposit> Items, int TotalCount)> GetFilteredAsync(AccountingTransactionFilter filter, CancellationToken cancellationToken = default)
    {
        var q = context.AccountingDeposits
            .Include(x => x.Account)
            .Include(x => x.VoucherHead)
            .AsQueryable();

        if (filter.AccountId.HasValue) q = q.Where(x => x.AccountId == filter.AccountId.Value);
        if (filter.FromDate.HasValue) q = q.Where(x => x.DepositDate >= filter.FromDate.Value.Date);
        if (filter.ToDate.HasValue) q = q.Where(x => x.DepositDate <= filter.ToDate.Value.Date);

        var total = await q.CountAsync(cancellationToken);
        var page = filter.Page < 1 ? 1 : filter.Page;
        var size = filter.PageSize is < 1 or > 500 ? 25 : filter.PageSize;
        var items = await q.OrderByDescending(x => x.DepositDate).Skip((page - 1) * size).Take(size).ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task<AccountingDeposit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.AccountingDeposits
            .Include(x => x.Account)
            .Include(x => x.VoucherHead)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<AccountingDeposit> AddAsync(AccountingDeposit entity, CancellationToken cancellationToken = default)
    {
        await context.AccountingDeposits.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(AccountingDeposit entity, CancellationToken cancellationToken = default)
    {
        context.AccountingDeposits.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(AccountingDeposit entity, CancellationToken cancellationToken = default)
    {
        context.AccountingDeposits.Remove(entity);
        return Task.CompletedTask;
    }
}
