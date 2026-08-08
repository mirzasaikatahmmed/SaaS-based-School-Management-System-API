using SchoolManagement.DAL.Entities.Master;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<Tenant?> GetBySchemaNameAsync(string schemaName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Tenant> AddAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);
    Task<SuperAdmin?> GetSuperAdminByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<SuperAdmin?> GetSuperAdminByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> SuperAdminExistsAsync(CancellationToken cancellationToken = default);
    Task<SuperAdmin> AddSuperAdminAsync(SuperAdmin admin, CancellationToken cancellationToken = default);
    Task UpdateSuperAdminAsync(SuperAdmin admin, CancellationToken cancellationToken = default);
}
