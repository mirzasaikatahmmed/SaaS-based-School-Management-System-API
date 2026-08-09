using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.OfficeAccounting;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class AccountingAccountService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http) : IAccountingAccountService
{
    public async Task<IReadOnlyList<AccountingAccountResponseDto>> GetAllAsync(bool? isActive, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        return (await uow.AccountingAccounts.GetAllAsync(isActive, ct)).Select(Map).ToList();
    }

    public async Task<AccountingAccountResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var x = await uow.AccountingAccounts.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Account '{id}' not found.");
        return Map(x);
    }

    public async Task<AccountingAccountResponseDto> CreateAsync(CreateAccountingAccountDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        if (string.IsNullOrWhiteSpace(dto.AccountName))
            throw new AppException("Account name is required.", 400);
        if (await uow.AccountingAccounts.NameExistsAsync(dto.AccountName.Trim(), null, ct))
            throw new ConflictException($"Account '{dto.AccountName}' already exists.");

        var x = new AccountingAccount
        {
            Id = Guid.NewGuid(),
            AccountName = dto.AccountName.Trim(),
            AccountNumber = dto.AccountNumber,
            Description = dto.Description,
            OpeningBalance = dto.OpeningBalance,
            CurrentBalance = dto.OpeningBalance,
            Date = dto.Date.HasValue ? DateTime.SpecifyKind(dto.Date.Value.Date, DateTimeKind.Utc) : DateTime.UtcNow.Date,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await uow.AccountingAccounts.AddAsync(x, ct);
        await uow.SaveTenantChangesAsync(ct);
        return Map(x);
    }

    public async Task<AccountingAccountResponseDto> UpdateAsync(Guid id, UpdateAccountingAccountDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var x = await uow.AccountingAccounts.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Account '{id}' not found.");
        if (string.IsNullOrWhiteSpace(dto.AccountName))
            throw new AppException("Account name is required.", 400);
        if (await uow.AccountingAccounts.NameExistsAsync(dto.AccountName.Trim(), id, ct))
            throw new ConflictException($"Account '{dto.AccountName}' already exists.");

        x.AccountName = dto.AccountName.Trim();
        x.AccountNumber = dto.AccountNumber;
        x.Description = dto.Description;
        if (dto.IsActive.HasValue) x.IsActive = dto.IsActive.Value;
        x.UpdatedAt = DateTime.UtcNow;
        await uow.AccountingAccounts.UpdateAsync(x, ct);
        await uow.SaveTenantChangesAsync(ct);
        return Map(x);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var x = await uow.AccountingAccounts.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Account '{id}' not found.");
        var deposits = await uow.AccountingDeposits.GetFilteredAsync(new AccountingTransactionFilter { AccountId = id, PageSize = 1 }, ct);
        var expenses = await uow.AccountingExpenses.GetFilteredAsync(new AccountingTransactionFilter { AccountId = id, PageSize = 1 }, ct);
        if (deposits.TotalCount > 0 || expenses.TotalCount > 0)
            throw new AppException("Cannot delete an account that has deposit or expense transactions.", 400);
        await uow.AccountingAccounts.DeleteAsync(x, ct);
        await uow.SaveTenantChangesAsync(ct);
    }

    public async Task<IReadOnlyList<AccountingAccountLookupDto>> GetLookupAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        return (await uow.AccountingAccounts.GetAllAsync(true, ct))
            .Select(x => new AccountingAccountLookupDto { Id = x.Id, AccountName = x.AccountName, CurrentBalance = x.CurrentBalance })
            .ToList();
    }

    public async Task<TransactionListResponseDto> GetTransactionsAsync(TransactionFilterDto filter, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var txFilter = new AccountingTransactionFilter
        {
            AccountId = filter.AccountId,
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            Page = 1,
            PageSize = 5000
        };

        var (deposits, _) = await uow.AccountingDeposits.GetFilteredAsync(txFilter, ct);
        var (expenses, _) = await uow.AccountingExpenses.GetFilteredAsync(txFilter, ct);
        var branch = tenant.TenantName ?? string.Empty;

        var raw = deposits.Select(d => new TxRow(
                d.Id, "Deposit", d.AccountId, d.Account?.AccountName ?? string.Empty,
                d.VoucherHeadId, d.VoucherHead?.Name ?? string.Empty,
                d.Amount, d.DepositDate, d.RefNo, d.PayVia, d.Description, d.CreatedAt))
            .Concat(expenses.Select(e => new TxRow(
                e.Id, "Expense", e.AccountId, e.Account?.AccountName ?? string.Empty,
                e.VoucherHeadId, e.VoucherHead?.Name ?? string.Empty,
                e.Amount, e.ExpenseDate, e.RefNo, e.PayVia, e.Description, e.CreatedAt)))
            .ToList();

        var accountOpeningBalances = (await uow.AccountingAccounts.GetAllAsync(null, ct))
            .ToDictionary(a => a.Id, a => a.OpeningBalance);

        // Chronological ASC per account to compute a running balance, mirroring a bank statement.
        var enriched = new List<(TxRow Row, TransactionListItemDto Dto)>();
        foreach (var group in raw.GroupBy(x => x.AccountId))
        {
            var running = accountOpeningBalances.GetValueOrDefault(group.Key, 0m);
            foreach (var tx in group.OrderBy(x => x.Date).ThenBy(x => x.CreatedAt))
            {
                var isDeposit = tx.Type == "Deposit";
                running += isDeposit ? tx.Amount : -tx.Amount;

                enriched.Add((tx, new TransactionListItemDto
                {
                    Id = tx.Id,
                    Type = tx.Type,
                    Branch = branch,
                    AccountName = tx.AccountName,
                    VoucherHead = tx.VoucherHead,
                    Amount = isDeposit ? tx.Amount : -tx.Amount,
                    Date = tx.Date,
                    RefNo = tx.RefNo,
                    PayVia = tx.PayVia,
                    Description = tx.Description,
                    Dr = isDeposit ? null : tx.Amount,
                    Cr = isDeposit ? tx.Amount : null,
                    RunningBalance = running
                }));
            }
        }

        IEnumerable<(TxRow Row, TransactionListItemDto Dto)> filtered = enriched;
        if (!string.IsNullOrWhiteSpace(filter.Type))
            filtered = filtered.Where(x => x.Dto.Type.Equals(filter.Type, StringComparison.OrdinalIgnoreCase));
        if (filter.VoucherHeadId.HasValue)
            filtered = filtered.Where(x => x.Row.VoucherHeadId == filter.VoucherHeadId.Value);

        var ordered = filtered.OrderByDescending(x => x.Dto.Date).Select(x => x.Dto).ToList();

        var page = filter.Page < 1 ? 1 : filter.Page;
        var size = filter.PageSize is < 1 or > 500 ? 25 : filter.PageSize;
        var total = ordered.Count;
        var pageItems = ordered.Skip((page - 1) * size).Take(size).ToList();

        return new TransactionListResponseDto
        {
            Data = pageItems,
            TotalCount = total,
            Page = page,
            PageSize = size,
            TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)size),
            TotalDeposits = deposits.Sum(d => d.Amount),
            TotalExpenses = expenses.Sum(e => e.Amount)
        };
    }

    private sealed record TxRow(
        Guid Id, string Type, Guid AccountId, string AccountName,
        Guid VoucherHeadId, string VoucherHead, decimal Amount, DateTime Date,
        string? RefNo, string? PayVia, string? Description, DateTime CreatedAt);

    private AccountingAccountResponseDto Map(AccountingAccount x) => new()
    {
        Id = x.Id,
        AccountName = x.AccountName,
        AccountNumber = x.AccountNumber,
        Description = x.Description,
        OpeningBalance = x.OpeningBalance,
        CurrentBalance = x.CurrentBalance,
        Date = x.Date,
        IsActive = x.IsActive,
        Branch = tenant.TenantName ?? string.Empty,
        CreatedAt = x.CreatedAt
    };

    private async Task Ready(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(tenant.SchemaName))
            throw new AppException("X-Tenant-ID header is required.", 400);
        await provisioner.EnsureStudentAndOfficeAccountingModuleAsync(tenant.SchemaName!, ct);
    }

    private HashSet<string> Roles()
    {
        var p = http.HttpContext?.User;
        if (p is null) return [];
        return p.FindAll("role").Concat(p.FindAll(ClaimTypes.Role)).Select(x => x.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private void Manage()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin) && !r.Contains(AppConstants.Roles.Accountant))
            throw new ForbiddenException("Only Super Admin, School Admin, or Accountant can manage accounting accounts.");
    }
}
