using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.StudentAccounting;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class OfflinePaymentTypeService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http) : IOfflinePaymentTypeService
{
    public async Task<IReadOnlyList<OfflinePaymentTypeResponseDto>> GetAllAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        return (await uow.OfflinePaymentTypes.GetAllAsync(true, ct)).Select(Map).ToList();
    }

    public async Task<OfflinePaymentTypeResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        var x = await uow.OfflinePaymentTypes.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Payment type '{id}' not found.");
        return Map(x);
    }

    public async Task<OfflinePaymentTypeResponseDto> CreateAsync(CreateOfflinePaymentTypeDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new AppException("Name is required.", 400);
        if (await uow.OfflinePaymentTypes.NameExistsAsync(dto.Name.Trim(), null, ct))
            throw new ConflictException($"Payment type '{dto.Name}' already exists.");

        var x = new OfflinePaymentType
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            Instructions = dto.Instructions,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await uow.OfflinePaymentTypes.AddAsync(x, ct);
        await uow.SaveTenantChangesAsync(ct);
        return Map(x);
    }

    public async Task<OfflinePaymentTypeResponseDto> UpdateAsync(Guid id, UpdateOfflinePaymentTypeDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var x = await uow.OfflinePaymentTypes.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Payment type '{id}' not found.");
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new AppException("Name is required.", 400);
        if (await uow.OfflinePaymentTypes.NameExistsAsync(dto.Name.Trim(), id, ct))
            throw new ConflictException($"Payment type '{dto.Name}' already exists.");

        x.Name = dto.Name.Trim();
        x.Instructions = dto.Instructions;
        if (dto.IsActive.HasValue) x.IsActive = dto.IsActive.Value;
        x.UpdatedAt = DateTime.UtcNow;
        await uow.OfflinePaymentTypes.UpdateAsync(x, ct);
        await uow.SaveTenantChangesAsync(ct);
        return Map(x);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var x = await uow.OfflinePaymentTypes.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Payment type '{id}' not found.");
        await uow.OfflinePaymentTypes.DeleteAsync(x, ct);
        await uow.SaveTenantChangesAsync(ct);
    }

    private static OfflinePaymentTypeResponseDto Map(OfflinePaymentType x) => new()
    {
        Id = x.Id,
        Name = x.Name,
        Instructions = x.Instructions,
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
            throw new ForbiddenException("Only Super Admin, School Admin, or Accountant can manage payment types.");
    }
}
