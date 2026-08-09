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

public class VoucherHeadService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http) : IVoucherHeadService
{
    public async Task<IReadOnlyList<VoucherHeadResponseDto>> GetAllAsync(string? type, CancellationToken ct = default)
    {
        await Ready(ct);
        var items = string.IsNullOrWhiteSpace(type)
            ? await uow.VoucherHeads.GetAllAsync(ct)
            : await uow.VoucherHeads.GetByTypeAsync(NormalizeType(type), ct);
        return items.Select(Map).ToList();
    }

    public async Task<VoucherHeadResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        var x = await uow.VoucherHeads.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Voucher head '{id}' not found.");
        return Map(x);
    }

    public async Task<VoucherHeadResponseDto> CreateAsync(CreateVoucherHeadDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new AppException("Name is required.", 400);
        var type = NormalizeType(dto.Type);
        if (await uow.VoucherHeads.NameExistsAsync(dto.Name.Trim(), null, ct))
            throw new ConflictException($"Voucher head '{dto.Name}' already exists.");

        var x = new VoucherHead
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            Type = type,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await uow.VoucherHeads.AddAsync(x, ct);
        await uow.SaveTenantChangesAsync(ct);
        return Map(x);
    }

    public async Task<VoucherHeadResponseDto> UpdateAsync(Guid id, UpdateVoucherHeadDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var x = await uow.VoucherHeads.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Voucher head '{id}' not found.");
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new AppException("Name is required.", 400);
        var type = NormalizeType(dto.Type);
        if (await uow.VoucherHeads.NameExistsAsync(dto.Name.Trim(), id, ct))
            throw new ConflictException($"Voucher head '{dto.Name}' already exists.");

        x.Name = dto.Name.Trim();
        x.Type = type;
        if (dto.IsActive.HasValue) x.IsActive = dto.IsActive.Value;
        await uow.VoucherHeads.UpdateAsync(x, ct);
        await uow.SaveTenantChangesAsync(ct);
        return Map(x);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var x = await uow.VoucherHeads.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Voucher head '{id}' not found.");
        await uow.VoucherHeads.DeleteAsync(x, ct);
        await uow.SaveTenantChangesAsync(ct);
    }

    private static string NormalizeType(string type)
    {
        var t = (type ?? string.Empty).Trim();
        if (t.Equals(VoucherHeadTypes.Income, StringComparison.OrdinalIgnoreCase)) return VoucherHeadTypes.Income;
        if (t.Equals(VoucherHeadTypes.Expense, StringComparison.OrdinalIgnoreCase)) return VoucherHeadTypes.Expense;
        throw new AppException("Type must be 'Income' or 'Expense'.", 400);
    }

    private static VoucherHeadResponseDto Map(VoucherHead x) => new()
    {
        Id = x.Id,
        Name = x.Name,
        Type = x.Type,
        IsActive = x.IsActive,
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
            throw new ForbiddenException("Only Super Admin, School Admin, or Accountant can manage voucher heads.");
    }
}
