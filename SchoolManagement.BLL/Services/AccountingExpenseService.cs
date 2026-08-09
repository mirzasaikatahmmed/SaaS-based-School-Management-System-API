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

public class AccountingExpenseService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IStorageService storage,
    IHttpContextAccessor http) : IAccountingExpenseService
{
    public async Task<AccountingExpenseListResponseDto> GetFilteredAsync(AccountingExpenseFilterDto filter, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var page = filter.Page < 1 ? 1 : filter.Page;
        var size = filter.PageSize is < 1 or > 200 ? 25 : filter.PageSize;
        var (items, total) = await uow.AccountingExpenses.GetFilteredAsync(new AccountingTransactionFilter
        {
            AccountId = filter.AccountId,
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            Page = page,
            PageSize = size
        }, ct);

        var data = new List<AccountingExpenseResponseDto>();
        foreach (var x in items) data.Add(await MapAsync(x, ct));

        return new AccountingExpenseListResponseDto
        {
            Data = data,
            TotalCount = total,
            Page = page,
            PageSize = size,
            TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)size)
        };
    }

    public async Task<AccountingExpenseResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var x = await uow.AccountingExpenses.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Expense '{id}' not found.");
        return await MapAsync(x, ct);
    }

    public async Task<AccountingExpenseResponseDto> CreateAsync(CreateAccountingExpenseDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        if (dto.Amount <= 0)
            throw new AppException("Amount must be greater than zero.", 400);

        var account = await uow.AccountingAccounts.GetByIdAsync(dto.AccountId, ct)
            ?? throw new NotFoundException($"Account '{dto.AccountId}' not found.");
        await ValidateVoucherHeadAsync(dto.VoucherHeadId, VoucherHeadTypes.Expense, ct);

        var x = new AccountingExpense
        {
            Id = Guid.NewGuid(),
            AccountId = dto.AccountId,
            VoucherHeadId = dto.VoucherHeadId,
            RefNo = dto.RefNo,
            Amount = dto.Amount,
            ExpenseDate = dto.ExpenseDate.HasValue ? DateTime.SpecifyKind(dto.ExpenseDate.Value.Date, DateTimeKind.Utc) : DateTime.UtcNow.Date,
            PayVia = dto.PayVia,
            Description = dto.Description,
            CreatedBy = CurrentUserOrNull(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await uow.AccountingExpenses.AddAsync(x, ct);

        account.CurrentBalance -= dto.Amount;
        account.UpdatedAt = DateTime.UtcNow;
        await uow.AccountingAccounts.UpdateAsync(account, ct);

        await uow.SaveTenantChangesAsync(ct);
        return await MapAsync(await uow.AccountingExpenses.GetByIdAsync(x.Id, ct) ?? x, ct);
    }

    public async Task<AccountingExpenseResponseDto> UpdateAsync(Guid id, UpdateAccountingExpenseDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        if (dto.Amount <= 0)
            throw new AppException("Amount must be greater than zero.", 400);

        var x = await uow.AccountingExpenses.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Expense '{id}' not found.");
        var newAccount = await uow.AccountingAccounts.GetByIdAsync(dto.AccountId, ct)
            ?? throw new NotFoundException($"Account '{dto.AccountId}' not found.");
        await ValidateVoucherHeadAsync(dto.VoucherHeadId, VoucherHeadTypes.Expense, ct);

        var oldAccountId = x.AccountId;
        var oldAmount = x.Amount;

        if (oldAccountId == dto.AccountId)
        {
            newAccount.CurrentBalance = newAccount.CurrentBalance + oldAmount - dto.Amount;
            newAccount.UpdatedAt = DateTime.UtcNow;
            await uow.AccountingAccounts.UpdateAsync(newAccount, ct);
        }
        else
        {
            var oldAccount = await uow.AccountingAccounts.GetByIdAsync(oldAccountId, ct);
            if (oldAccount is not null)
            {
                oldAccount.CurrentBalance += oldAmount;
                oldAccount.UpdatedAt = DateTime.UtcNow;
                await uow.AccountingAccounts.UpdateAsync(oldAccount, ct);
            }
            newAccount.CurrentBalance -= dto.Amount;
            newAccount.UpdatedAt = DateTime.UtcNow;
            await uow.AccountingAccounts.UpdateAsync(newAccount, ct);
        }

        x.AccountId = dto.AccountId;
        x.VoucherHeadId = dto.VoucherHeadId;
        x.RefNo = dto.RefNo;
        x.Amount = dto.Amount;
        x.ExpenseDate = DateTime.SpecifyKind(dto.ExpenseDate.Date, DateTimeKind.Utc);
        x.PayVia = dto.PayVia;
        x.Description = dto.Description;
        x.UpdatedAt = DateTime.UtcNow;
        await uow.AccountingExpenses.UpdateAsync(x, ct);
        await uow.SaveTenantChangesAsync(ct);
        return await MapAsync(await uow.AccountingExpenses.GetByIdAsync(id, ct) ?? x, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var x = await uow.AccountingExpenses.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Expense '{id}' not found.");
        var account = await uow.AccountingAccounts.GetByIdAsync(x.AccountId, ct);
        if (account is not null)
        {
            account.CurrentBalance += x.Amount;
            account.UpdatedAt = DateTime.UtcNow;
            await uow.AccountingAccounts.UpdateAsync(account, ct);
        }

        if (!string.IsNullOrWhiteSpace(x.AttachmentUrl) && !string.IsNullOrWhiteSpace(tenant.TenantSlug))
        {
            try { await storage.DeleteFileAsync(tenant.TenantSlug, x.AttachmentUrl, ct); } catch { /* ignore */ }
        }

        await uow.AccountingExpenses.DeleteAsync(x, ct);
        await uow.SaveTenantChangesAsync(ct);
    }

    public async Task<AccountingExpenseResponseDto> UploadAttachmentAsync(Guid id, Stream stream, string fileName, string contentType, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var x = await uow.AccountingExpenses.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Expense '{id}' not found.");

        if (stream.CanSeek && stream.Length > 5 * 1024 * 1024)
            throw new AppException("Attachment must be 5MB or smaller.", 400);

        var slug = tenant.TenantSlug ?? throw new AppException("Tenant slug is not resolved.", 400);
        var safeName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safeName)) safeName = "attachment.bin";
        var key = $"{AppConstants.StorageFolders.AccountingExpenses}/{id}/{safeName}";

        if (!string.IsNullOrWhiteSpace(x.AttachmentUrl))
        {
            try { await storage.DeleteFileAsync(slug, x.AttachmentUrl, ct); } catch { /* ignore */ }
        }

        await storage.UploadObjectAsync(slug, key, stream, contentType, ct);
        x.AttachmentUrl = key;
        x.UpdatedAt = DateTime.UtcNow;
        await uow.AccountingExpenses.UpdateAsync(x, ct);
        await uow.SaveTenantChangesAsync(ct);
        return await MapAsync(await uow.AccountingExpenses.GetByIdAsync(id, ct) ?? x, ct);
    }

    private async Task<VoucherHead> ValidateVoucherHeadAsync(Guid voucherHeadId, string expectedType, CancellationToken ct)
    {
        var voucherHead = await uow.VoucherHeads.GetByIdAsync(voucherHeadId, ct)
            ?? throw new NotFoundException($"Voucher head '{voucherHeadId}' not found.");
        if (!voucherHead.Type.Equals(expectedType, StringComparison.OrdinalIgnoreCase))
            throw new AppException($"Voucher head '{voucherHead.Name}' must be of type '{expectedType}'.", 400);
        return voucherHead;
    }

    private async Task<AccountingExpenseResponseDto> MapAsync(AccountingExpense x, CancellationToken ct) => new()
    {
        Id = x.Id,
        AccountId = x.AccountId,
        AccountName = x.Account?.AccountName ?? string.Empty,
        VoucherHeadId = x.VoucherHeadId,
        VoucherHeadName = x.VoucherHead?.Name ?? string.Empty,
        RefNo = x.RefNo,
        Amount = x.Amount,
        ExpenseDate = x.ExpenseDate,
        PayVia = x.PayVia,
        Description = x.Description,
        AttachmentUrl = await Presign(x.AttachmentUrl, ct),
        CreatedAt = x.CreatedAt
    };

    private async Task<string?> Presign(string? key, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(tenant.TenantSlug)) return key;
        try { return await storage.GetPresignedUrlAsync(tenant.TenantSlug, key, ct); }
        catch { return key; }
    }

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
            throw new ForbiddenException("Only Super Admin, School Admin, or Accountant can manage expenses.");
    }

    private Guid? CurrentUserOrNull()
    {
        var c = http.HttpContext?.User.FindFirst(AppConstants.Claims.UserId)
            ?? http.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        return c is not null && Guid.TryParse(c.Value, out var id) ? id : null;
    }
}
