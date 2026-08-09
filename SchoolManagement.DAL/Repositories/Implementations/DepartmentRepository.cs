using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly TenantDbContext _context;

    public DepartmentRepository(TenantDbContext context) => _context = context;

    public async Task<IReadOnlyList<Department>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Departments.OrderBy(d => d.CreatedAt).ToListAsync(cancellationToken);

    public async Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Departments.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public async Task<Department?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var n = name.Trim().ToUpperInvariant();
        return await _context.Departments.FirstOrDefaultAsync(d => d.Name.ToUpper() == n, cancellationToken);
    }

    public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var n = name.Trim().ToUpperInvariant();
        var q = _context.Departments.Where(d => d.Name.ToUpper() == n);
        if (excludeId.HasValue) q = q.Where(d => d.Id != excludeId.Value);
        return await q.AnyAsync(cancellationToken);
    }

    public async Task<int> CountEmployeesUsingAsync(Guid departmentId, CancellationToken cancellationToken = default)
        => await _context.Employees.CountAsync(e => e.DepartmentId == departmentId && e.IsActive, cancellationToken);

    public async Task<Department> AddAsync(Department entity, CancellationToken cancellationToken = default)
    {
        await _context.Departments.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(Department entity, CancellationToken cancellationToken = default)
    {
        _context.Departments.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Department entity, CancellationToken cancellationToken = default)
    {
        _context.Departments.Remove(entity);
        return Task.CompletedTask;
    }
}
