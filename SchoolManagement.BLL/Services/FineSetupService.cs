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

public class FineSetupService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http) : IFineSetupService
{
    public async Task<IReadOnlyList<FineSetupResponseDto>> GetAllAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        return (await uow.FineSetups.GetAllAsync(ct)).Select(Map).ToList();
    }

    public async Task<FineSetupResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        var x = await uow.FineSetups.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Fine setup '{id}' not found.");
        return Map(x);
    }

    public async Task<FineSetupResponseDto> CreateAsync(CreateFineSetupDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var group = await uow.FeesGroups.GetByIdAsync(dto.GroupId, ct) ?? throw new NotFoundException($"Fees group '{dto.GroupId}' not found.");
        var feesType = await uow.FeesTypes.GetByIdAsync(dto.FeesTypeId, ct) ?? throw new NotFoundException($"Fees type '{dto.FeesTypeId}' not found.");
        if (await uow.FineSetups.ExistsAsync(dto.GroupId, dto.FeesTypeId, null, ct))
            throw new ConflictException($"A fine setup already exists for group '{group.Name}' and fees type '{feesType.Name}'.");
        if (dto.FineValue < 0)
            throw new AppException("Fine value cannot be negative.", 400);

        var x = new FineSetup
        {
            Id = Guid.NewGuid(),
            GroupId = dto.GroupId,
            FeesTypeId = dto.FeesTypeId,
            FineType = dto.FineType.Trim(),
            FineValue = dto.FineValue,
            LateFeeFrequency = dto.LateFeeFrequency,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await uow.FineSetups.AddAsync(x, ct);
        await uow.SaveTenantChangesAsync(ct);
        return Map(await uow.FineSetups.GetByIdAsync(x.Id, ct) ?? x);
    }

    public async Task<FineSetupResponseDto> UpdateAsync(Guid id, UpdateFineSetupDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var x = await uow.FineSetups.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Fine setup '{id}' not found.");
        if (dto.FineValue < 0)
            throw new AppException("Fine value cannot be negative.", 400);

        x.FineType = dto.FineType.Trim();
        x.FineValue = dto.FineValue;
        x.LateFeeFrequency = dto.LateFeeFrequency;
        if (dto.IsActive.HasValue) x.IsActive = dto.IsActive.Value;
        x.UpdatedAt = DateTime.UtcNow;
        await uow.FineSetups.UpdateAsync(x, ct);
        await uow.SaveTenantChangesAsync(ct);
        return Map(await uow.FineSetups.GetByIdAsync(id, ct) ?? x);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var x = await uow.FineSetups.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Fine setup '{id}' not found.");
        await uow.FineSetups.DeleteAsync(x, ct);
        await uow.SaveTenantChangesAsync(ct);
    }

    private static FineSetupResponseDto Map(FineSetup x) => new()
    {
        Id = x.Id,
        GroupId = x.GroupId,
        GroupName = x.Group?.Name ?? string.Empty,
        FeesTypeId = x.FeesTypeId,
        FeesTypeName = x.FeesType?.Name ?? string.Empty,
        FineType = x.FineType,
        FineValue = x.FineValue,
        LateFeeFrequency = x.LateFeeFrequency,
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
            throw new ForbiddenException("Only Super Admin, School Admin, or Accountant can manage fine setups.");
    }
}
