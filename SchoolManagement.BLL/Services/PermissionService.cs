using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class PermissionService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner) : IPermissionService
{
    public async Task<bool> HasPermissionAsync(
        Guid userId,
        IEnumerable<string> rolePrefixes,
        string featureKey,
        string action,
        CancellationToken cancellationToken = default)
    {
        var roles = rolePrefixes.Select(r => r.Trim().ToLowerInvariant()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (roles.Contains(AppConstants.Roles.SuperAdmin))
            return true;

        if (string.IsNullOrEmpty(tenant.SchemaName))
            return false;

        await provisioner.EnsureSettingsModuleAsync(tenant.SchemaName!, cancellationToken);

        if (roles.Contains(AppConstants.Roles.Admin))
        {
            var admin = await uow.Roles.GetByPrefixAsync(AppConstants.Roles.Admin, cancellationToken);
            if (admin is null)
                return true;

            var adminPerms = await uow.Roles.GetPermissionsAsync(admin.Id, cancellationToken);
            if (adminPerms.Count == 0)
                return true; // unsaved Admin matrix = full access
        }

        var perms = await uow.Roles.GetPermissionsForRolePrefixesAsync(roles, cancellationToken);
        var matching = perms.Where(p => p.FeatureKey.Equals(featureKey, StringComparison.OrdinalIgnoreCase)).ToList();
        if (matching.Count == 0)
            return false;

        return action switch
        {
            AppConstants.PermissionActions.View => matching.Any(p => p.CanView),
            AppConstants.PermissionActions.Add => matching.Any(p => p.CanAdd),
            AppConstants.PermissionActions.Edit => matching.Any(p => p.CanEdit),
            AppConstants.PermissionActions.Delete => matching.Any(p => p.CanDelete),
            _ => false
        };
    }
}
