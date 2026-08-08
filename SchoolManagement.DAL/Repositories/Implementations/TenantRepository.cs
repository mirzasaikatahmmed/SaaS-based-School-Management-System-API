using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Master;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class TenantRepository : ITenantRepository
{
    private readonly MasterDbContext _context;

    public TenantRepository(MasterDbContext context)
    {
        _context = context;
    }

    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .FirstOrDefaultAsync(t => t.Slug == slug.ToLowerInvariant(), cancellationToken);
    }

    public async Task<Tenant?> GetBySchemaNameAsync(string schemaName, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .FirstOrDefaultAsync(t => t.SchemaName == schemaName, cancellationToken);
    }

    public async Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Tenant> AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        await _context.Tenants.AddAsync(tenant, cancellationToken);
        return tenant;
    }

    public Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        tenant.UpdatedAt = DateTime.UtcNow;
        _context.Tenants.Update(tenant);
        return Task.CompletedTask;
    }

    public async Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .AnyAsync(t => t.Slug == slug.ToLowerInvariant(), cancellationToken);
    }

    public async Task<SuperAdmin?> GetSuperAdminByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.SuperAdmins
            .FirstOrDefaultAsync(a => a.Email == email.ToLowerInvariant(), cancellationToken);
    }

    public async Task<SuperAdmin?> GetSuperAdminByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.SuperAdmins.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<bool> SuperAdminExistsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SuperAdmins.AnyAsync(cancellationToken);
    }

    public async Task<SuperAdmin> AddSuperAdminAsync(SuperAdmin admin, CancellationToken cancellationToken = default)
    {
        await _context.SuperAdmins.AddAsync(admin, cancellationToken);
        return admin;
    }

    public Task UpdateSuperAdminAsync(SuperAdmin admin, CancellationToken cancellationToken = default)
    {
        admin.UpdatedAt = DateTime.UtcNow;
        _context.SuperAdmins.Update(admin);
        return Task.CompletedTask;
    }
}
