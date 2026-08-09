using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.Settings;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class RoleService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http) : IRoleService
{
    public async Task<IReadOnlyList<RoleResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        Manage();
        return (await uow.Roles.GetAllAsync(cancellationToken)).Select(Map).ToList();
    }

    public async Task<RoleResponseDto> CreateAsync(CreateRoleDto dto, CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        Manage();
        var name = dto.Name.Trim();
        if (await uow.Roles.NameExistsAsync(name, null, cancellationToken))
            throw new ConflictException($"Role '{name}' already exists.");

        var prefix = Slugify(name);
        if (await uow.Roles.PrefixExistsAsync(prefix, null, cancellationToken))
            prefix = $"{prefix}_{Guid.NewGuid().ToString("N")[..6]}";

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = name,
            Prefix = prefix,
            IsSystem = false,
            IsActive = true,
            Description = dto.Description?.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        await uow.Roles.AddAsync(role, cancellationToken);
        await uow.SaveTenantChangesAsync(cancellationToken);
        return Map(role);
    }

    public async Task<RoleResponseDto> UpdateAsync(Guid id, UpdateRoleDto dto, CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        Manage();
        var role = await uow.Roles.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Role '{id}' not found.");

        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            var name = dto.Name.Trim();
            if (await uow.Roles.NameExistsAsync(name, id, cancellationToken))
                throw new ConflictException($"Role '{name}' already exists.");
            role.Name = name;
        }

        if (dto.Description is not null)
            role.Description = dto.Description.Trim();

        if (dto.IsActive.HasValue)
            role.IsActive = dto.IsActive.Value;

        await uow.Roles.UpdateAsync(role, cancellationToken);
        await uow.SaveTenantChangesAsync(cancellationToken);
        return Map(role);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        Manage();
        var role = await uow.Roles.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Role '{id}' not found.");

        if (role.IsSystem)
            throw new AppException("System roles cannot be deleted.", 400);

        var users = await uow.Roles.CountUsersAsync(id, cancellationToken);
        if (users > 0)
            throw new ConflictException($"Role is assigned to {users} user(s) and cannot be deleted.");

        await uow.Roles.DeleteAsync(role, cancellationToken);
        await uow.SaveTenantChangesAsync(cancellationToken);
    }

    public async Task<RolePermissionMatrixDto> GetPermissionsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        Manage();
        var role = await uow.Roles.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Role '{id}' not found.");

        var saved = (await uow.Roles.GetPermissionsAsync(id, cancellationToken))
            .ToDictionary(p => p.FeatureKey, StringComparer.OrdinalIgnoreCase);

        // Admin with no rows yet → treat as full access in UI (seed on first save)
        var isAdminFull = role.Prefix.Equals(AppConstants.Roles.Admin, StringComparison.OrdinalIgnoreCase) && saved.Count == 0;

        var items = AppFeatures.All.Select(f =>
        {
            saved.TryGetValue(f.Key, out var row);
            var view = isAdminFull || (row?.CanView ?? false);
            var add = !f.ViewOnly && (isAdminFull || (row?.CanAdd ?? false));
            var edit = !f.ViewOnly && (isAdminFull || (row?.CanEdit ?? false));
            var del = !f.ViewOnly && (isAdminFull || (row?.CanDelete ?? false));
            return new RolePermissionItemDto
            {
                FeatureKey = f.Key,
                Module = f.Module,
                Name = f.Name,
                ViewOnly = f.ViewOnly,
                CanView = view,
                CanAdd = add,
                CanEdit = edit,
                CanDelete = del
            };
        }).ToList();

        return new RolePermissionMatrixDto { RoleId = role.Id, RoleName = role.Name, Permissions = items };
    }

    public async Task<RolePermissionMatrixDto> UpdatePermissionsAsync(Guid id, UpdateRolePermissionsDto dto, CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        Manage();
        var role = await uow.Roles.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Role '{id}' not found.");

        var incoming = new List<RolePermission>();
        foreach (var item in dto.Permissions)
        {
            if (!AppFeatures.IsValidKey(item.FeatureKey))
                throw new AppException($"Unknown feature key '{item.FeatureKey}'.", 400);

            var def = AppFeatures.ByKey[item.FeatureKey];
            incoming.Add(new RolePermission
            {
                FeatureKey = item.FeatureKey,
                CanView = item.CanView,
                CanAdd = def.ViewOnly ? false : item.CanAdd,
                CanEdit = def.ViewOnly ? false : item.CanEdit,
                CanDelete = def.ViewOnly ? false : item.CanDelete
            });
        }

        await uow.BeginTenantTransactionAsync(cancellationToken);
        try
        {
            await uow.Roles.UpsertPermissionsAsync(id, incoming, cancellationToken);
            await uow.SaveTenantChangesAsync(cancellationToken);
            await uow.CommitTenantTransactionAsync(cancellationToken);
        }
        catch
        {
            await uow.RollbackTenantTransactionAsync(cancellationToken);
            throw;
        }

        return await GetPermissionsAsync(id, cancellationToken);
    }

    private static RoleResponseDto Map(Role r) => new()
    {
        Id = r.Id,
        Name = r.Name,
        Prefix = r.Prefix,
        IsSystemRole = r.IsSystem,
        IsActive = r.IsActive,
        Description = r.Description,
        CreatedAt = r.CreatedAt
    };

    private static string Slugify(string name)
    {
        var slug = Regex.Replace(name.Trim().ToLowerInvariant(), @"[^a-z0-9]+", "_").Trim('_');
        return string.IsNullOrEmpty(slug) ? "role" : slug;
    }

    private async Task Ready(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(tenant.SchemaName))
            throw new AppException("X-Tenant-ID header is required.", 400);
        await provisioner.EnsureSettingsModuleAsync(tenant.SchemaName!, ct);
        await uow.Users.SeedRolesAsync(ct);
        await uow.SaveTenantChangesAsync(ct);
    }

    private HashSet<string> Roles() =>
        http.HttpContext?.User.FindAll("role").Concat(http.HttpContext.User.FindAll(ClaimTypes.Role))
            .Select(x => x.Value).ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];

    private void Manage()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin))
            throw new ForbiddenException("Only Super Admin or School Admin can manage roles.");
    }
}
