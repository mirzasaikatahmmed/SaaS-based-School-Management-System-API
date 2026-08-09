using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IRoleRepository
{
    Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Role?> GetByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> PrefixExistsAsync(string prefix, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<int> CountUsersAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<Role> AddAsync(Role entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Role entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Role entity, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RolePermission>> GetPermissionsAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task UpsertPermissionsAsync(Guid roleId, IEnumerable<RolePermission> permissions, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RolePermission>> GetPermissionsForRolePrefixesAsync(IEnumerable<string> prefixes, CancellationToken cancellationToken = default);
}
