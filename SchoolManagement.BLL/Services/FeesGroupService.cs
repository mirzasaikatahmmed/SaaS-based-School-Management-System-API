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

public class FeesGroupService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http) : IFeesGroupService
{
    public async Task<IReadOnlyList<FeesGroupResponseDto>> GetAllAsync(bool? isActive, CancellationToken ct = default)
    {
        await Ready(ct);
        return (await uow.FeesGroups.GetAllAsync(isActive, ct)).Select(Map).ToList();
    }

    public async Task<FeesGroupResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        var x = await uow.FeesGroups.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Fees group '{id}' not found.");
        return Map(x);
    }

    public async Task<FeesGroupResponseDto> CreateAsync(CreateFeesGroupDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new AppException("Name is required.", 400);
        if (await uow.FeesGroups.NameExistsAsync(dto.Name.Trim(), null, ct))
            throw new ConflictException($"Fees group '{dto.Name}' already exists.");
        await ValidateItems(dto.Items, ct);

        var group = new FeesGroup
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            Description = dto.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await uow.FeesGroups.AddAsync(group, ct);
        await uow.SaveTenantChangesAsync(ct);

        await uow.FeesGroups.ReplaceItemsAsync(group.Id, dto.Items.Select(i => ToEntity(group.Id, i)), ct);
        await uow.SaveTenantChangesAsync(ct);

        return Map(await uow.FeesGroups.GetByIdAsync(group.Id, ct) ?? group);
    }

    public async Task<FeesGroupResponseDto> UpdateAsync(Guid id, UpdateFeesGroupDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var group = await uow.FeesGroups.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Fees group '{id}' not found.");
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new AppException("Name is required.", 400);
        if (await uow.FeesGroups.NameExistsAsync(dto.Name.Trim(), id, ct))
            throw new ConflictException($"Fees group '{dto.Name}' already exists.");
        await ValidateItems(dto.Items, ct);

        group.Name = dto.Name.Trim();
        group.Description = dto.Description;
        if (dto.IsActive.HasValue) group.IsActive = dto.IsActive.Value;
        group.UpdatedAt = DateTime.UtcNow;
        await uow.FeesGroups.UpdateAsync(group, ct);
        await uow.FeesGroups.ReplaceItemsAsync(group.Id, dto.Items.Select(i => ToEntity(group.Id, i)), ct);
        await uow.SaveTenantChangesAsync(ct);

        return Map(await uow.FeesGroups.GetByIdAsync(id, ct) ?? group);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var group = await uow.FeesGroups.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Fees group '{id}' not found.");
        var count = await uow.FeesGroups.CountAllocationsUsingAsync(id, ct);
        if (count > 0)
            throw new AppException($"Fees group is used by {count} allocation(s) and cannot be deleted.", 400);
        await uow.FeesGroups.DeleteAsync(group, ct);
        await uow.SaveTenantChangesAsync(ct);
    }

    public async Task<IReadOnlyList<FeesGroupLookupDto>> GetLookupAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        return (await uow.FeesGroups.GetAllAsync(true, ct))
            .Select(g => new FeesGroupLookupDto { Id = g.Id, Name = g.Name, TotalAmount = g.Items.Sum(i => i.Amount) })
            .ToList();
    }

    private async Task ValidateItems(List<FeesGroupItemDto> items, CancellationToken ct)
    {
        if (items.Count == 0)
            throw new AppException("At least one fee item is required.", 400);
        foreach (var i in items)
        {
            if (i.Amount <= 0)
                throw new AppException("Fee item amount must be greater than zero.", 400);
            var feesType = await uow.FeesTypes.GetByIdAsync(i.FeesTypeId, ct)
                ?? throw new NotFoundException($"Fees type '{i.FeesTypeId}' not found.");
            if (!feesType.IsActive)
                throw new AppException($"Fees type '{feesType.Name}' is inactive.", 400);
        }
    }

    private static FeesGroupItem ToEntity(Guid groupId, FeesGroupItemDto dto) => new()
    {
        Id = Guid.NewGuid(),
        GroupId = groupId,
        FeesTypeId = dto.FeesTypeId,
        DueDate = DateTime.SpecifyKind(dto.DueDate.Date, DateTimeKind.Utc),
        Amount = dto.Amount,
        SortOrder = dto.SortOrder,
        CreatedAt = DateTime.UtcNow
    };

    private static FeesGroupResponseDto Map(FeesGroup g) => new()
    {
        Id = g.Id,
        Name = g.Name,
        Description = g.Description,
        TotalAmount = g.Items.Sum(i => i.Amount),
        IsActive = g.IsActive,
        CreatedAt = g.CreatedAt,
        Items = g.Items.OrderBy(i => i.SortOrder).Select(i => new FeesGroupItemResponseDto
        {
            Id = i.Id,
            FeesTypeId = i.FeesTypeId,
            FeesTypeName = i.FeesType?.Name ?? string.Empty,
            DueDate = i.DueDate,
            Amount = i.Amount,
            SortOrder = i.SortOrder
        }).ToList()
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
            throw new ForbiddenException("Only Super Admin, School Admin, or Accountant can manage fees groups.");
    }
}
