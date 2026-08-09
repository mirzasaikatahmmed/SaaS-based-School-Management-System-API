using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class RoleRepository(TenantDbContext context) : IRoleRepository
{
    public async Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.Roles.OrderBy(r => r.Name).ToListAsync(cancellationToken);

    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Roles.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<Role?> GetByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        => await context.Roles.FirstOrDefaultAsync(r => r.Prefix == prefix, cancellationToken);

    public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var n = name.Trim().ToUpperInvariant();
        var q = context.Roles.Where(r => r.Name.ToUpper() == n);
        if (excludeId.HasValue) q = q.Where(r => r.Id != excludeId.Value);
        return await q.AnyAsync(cancellationToken);
    }

    public async Task<bool> PrefixExistsAsync(string prefix, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var p = prefix.Trim().ToLowerInvariant();
        var q = context.Roles.Where(r => r.Prefix == p);
        if (excludeId.HasValue) q = q.Where(r => r.Id != excludeId.Value);
        return await q.AnyAsync(cancellationToken);
    }

    public async Task<int> CountUsersAsync(Guid roleId, CancellationToken cancellationToken = default)
        => await context.UserRoles.CountAsync(ur => ur.RoleId == roleId, cancellationToken);

    public async Task<Role> AddAsync(Role entity, CancellationToken cancellationToken = default)
    {
        await context.Roles.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(Role entity, CancellationToken cancellationToken = default)
    {
        context.Roles.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Role entity, CancellationToken cancellationToken = default)
    {
        context.Roles.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<RolePermission>> GetPermissionsAsync(Guid roleId, CancellationToken cancellationToken = default)
        => await context.RolePermissions.Where(p => p.RoleId == roleId).ToListAsync(cancellationToken);

    public async Task UpsertPermissionsAsync(Guid roleId, IEnumerable<RolePermission> permissions, CancellationToken cancellationToken = default)
    {
        var existing = await context.RolePermissions.Where(p => p.RoleId == roleId).ToListAsync(cancellationToken);
        var byKey = existing.ToDictionary(p => p.FeatureKey, StringComparer.OrdinalIgnoreCase);
        var now = DateTime.UtcNow;

        foreach (var incoming in permissions)
        {
            if (byKey.TryGetValue(incoming.FeatureKey, out var row))
            {
                row.CanView = incoming.CanView;
                row.CanAdd = incoming.CanAdd;
                row.CanEdit = incoming.CanEdit;
                row.CanDelete = incoming.CanDelete;
                row.UpdatedAt = now;
            }
            else
            {
                await context.RolePermissions.AddAsync(new RolePermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = roleId,
                    FeatureKey = incoming.FeatureKey,
                    CanView = incoming.CanView,
                    CanAdd = incoming.CanAdd,
                    CanEdit = incoming.CanEdit,
                    CanDelete = incoming.CanDelete,
                    CreatedAt = now,
                    UpdatedAt = now
                }, cancellationToken);
            }
        }
    }

    public async Task<IReadOnlyList<RolePermission>> GetPermissionsForRolePrefixesAsync(
        IEnumerable<string> prefixes, CancellationToken cancellationToken = default)
    {
        var set = prefixes.Select(p => p.Trim().ToLowerInvariant()).ToHashSet();
        return await context.RolePermissions
            .Include(p => p.Role)
            .Where(p => set.Contains(p.Role.Prefix))
            .ToListAsync(cancellationToken);
    }
}
