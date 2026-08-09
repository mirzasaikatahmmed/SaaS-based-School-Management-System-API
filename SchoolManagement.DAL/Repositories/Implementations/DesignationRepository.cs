using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class DesignationRepository : IDesignationRepository
{
    private readonly TenantDbContext _context;

    public DesignationRepository(TenantDbContext context) => _context = context;

    public async Task<IReadOnlyList<Designation>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Designations.OrderBy(d => d.CreatedAt).ToListAsync(cancellationToken);

    public async Task<Designation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Designations.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public async Task<Designation?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var n = name.Trim().ToUpperInvariant();
        return await _context.Designations.FirstOrDefaultAsync(d => d.Name.ToUpper() == n, cancellationToken);
    }

    public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var n = name.Trim().ToUpperInvariant();
        var q = _context.Designations.Where(d => d.Name.ToUpper() == n);
        if (excludeId.HasValue) q = q.Where(d => d.Id != excludeId.Value);
        return await q.AnyAsync(cancellationToken);
    }

    public async Task<int> CountEmployeesUsingAsync(Guid designationId, CancellationToken cancellationToken = default)
        => await _context.Employees.CountAsync(e => e.DesignationId == designationId && e.IsActive, cancellationToken);

    public async Task<Designation> AddAsync(Designation entity, CancellationToken cancellationToken = default)
    {
        await _context.Designations.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(Designation entity, CancellationToken cancellationToken = default)
    {
        _context.Designations.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Designation entity, CancellationToken cancellationToken = default)
    {
        _context.Designations.Remove(entity);
        return Task.CompletedTask;
    }
}
