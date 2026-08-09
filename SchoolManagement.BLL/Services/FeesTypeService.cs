using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.StudentAccounting;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Helpers;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class FeesTypeService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http) : IFeesTypeService
{
    public async Task<IReadOnlyList<FeesTypeResponseDto>> GetAllAsync(bool? isActive, CancellationToken ct = default)
    {
        await Ready(ct);
        return (await uow.FeesTypes.GetAllAsync(isActive, ct)).Select(Map).ToList();
    }

    public async Task<FeesTypeResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        var x = await uow.FeesTypes.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Fees type '{id}' not found.");
        return Map(x);
    }

    public async Task<FeesTypeResponseDto> CreateAsync(CreateFeesTypeDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new AppException("Name is required.", 400);

        var code = AccountingHelpers.Slugify(string.IsNullOrWhiteSpace(dto.FeeCode) ? dto.Name : dto.FeeCode);
        if (string.IsNullOrWhiteSpace(code))
            throw new AppException("Fee code could not be generated from the provided name.", 400);
        if (await uow.FeesTypes.FeeCodeExistsAsync(code, null, ct))
            throw new ConflictException($"Fee code '{code}' already exists.");

        var x = new FeesType
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            FeeCode = code,
            Description = dto.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await uow.FeesTypes.AddAsync(x, ct);
        await uow.SaveTenantChangesAsync(ct);
        return Map(x);
    }

    public async Task<FeesTypeResponseDto> UpdateAsync(Guid id, UpdateFeesTypeDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var x = await uow.FeesTypes.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Fees type '{id}' not found.");
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new AppException("Name is required.", 400);

        var code = AccountingHelpers.Slugify(string.IsNullOrWhiteSpace(dto.FeeCode) ? dto.Name : dto.FeeCode);
        if (await uow.FeesTypes.FeeCodeExistsAsync(code, id, ct))
            throw new ConflictException($"Fee code '{code}' already exists.");

        x.Name = dto.Name.Trim();
        x.FeeCode = code;
        x.Description = dto.Description;
        if (dto.IsActive.HasValue) x.IsActive = dto.IsActive.Value;
        x.UpdatedAt = DateTime.UtcNow;
        await uow.FeesTypes.UpdateAsync(x, ct);
        await uow.SaveTenantChangesAsync(ct);
        return Map(x);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var x = await uow.FeesTypes.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Fees type '{id}' not found.");
        var count = await uow.FeesTypes.CountGroupItemsUsingAsync(id, ct);
        if (count > 0)
            throw new AppException($"Fees type is used by {count} fees group item(s) and cannot be deleted.", 400);
        await uow.FeesTypes.DeleteAsync(x, ct);
        await uow.SaveTenantChangesAsync(ct);
    }

    public async Task<IReadOnlyList<FeesTypeLookupDto>> GetLookupAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        return (await uow.FeesTypes.GetAllAsync(true, ct))
            .Select(x => new FeesTypeLookupDto { Id = x.Id, Name = x.Name, FeeCode = x.FeeCode })
            .ToList();
    }

    private static FeesTypeResponseDto Map(FeesType x) => new()
    {
        Id = x.Id,
        Name = x.Name,
        FeeCode = x.FeeCode,
        Description = x.Description,
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
            throw new ForbiddenException("Only Super Admin, School Admin, or Accountant can manage fees types.");
    }
}
