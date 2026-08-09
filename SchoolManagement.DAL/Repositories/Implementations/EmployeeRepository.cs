using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly TenantDbContext _context;

    public EmployeeRepository(TenantDbContext context) => _context = context;

    public async Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Employees.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<Employee?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Employees
            .Include(e => e.User)
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<Employee?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _context.Employees
            .Include(e => e.User)
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .FirstOrDefaultAsync(e => e.UserId == userId, cancellationToken);

    public async Task<(IReadOnlyList<Employee> Items, int TotalCount)> SearchAsync(
        EmployeeSearchFilter filter,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Employees
            .Include(e => e.User)
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .AsQueryable();

        if (filter.IsActive.HasValue)
            query = query.Where(e => e.IsActive == filter.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(filter.Role))
            query = query.Where(e => e.Role.ToLower() == filter.Role.Trim().ToLower());

        if (filter.DepartmentId.HasValue)
            query = query.Where(e => e.DepartmentId == filter.DepartmentId.Value);

        if (filter.DesignationId.HasValue)
            query = query.Where(e => e.DesignationId == filter.DesignationId.Value);

        if (filter.IsLoginActive.HasValue)
            query = query.Where(e => e.User.Active == filter.IsLoginActive.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLowerInvariant();
            query = query.Where(e =>
                e.Name.ToLower().Contains(term) ||
                e.StaffId.ToLower().Contains(term) ||
                e.Email.ToLower().Contains(term) ||
                e.MobileNo.Contains(term));
        }

        var total = await query.CountAsync(cancellationToken);
        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize is < 1 or > 10_000 ? 20 : filter.PageSize;

        query = ApplySort(query, filter.SortBy, filter.SortDir);

        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task<bool> StaffIdExistsAsync(string staffId, CancellationToken cancellationToken = default)
        => await _context.Employees.AnyAsync(e => e.StaffId == staffId, cancellationToken);

    public async Task<bool> EmailExistsAsync(string email, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var q = _context.Employees.Where(e => e.Email.ToLower() == normalized);
        if (excludeId.HasValue) q = q.Where(e => e.Id != excludeId.Value);
        return await q.AnyAsync(cancellationToken);
    }

    public async Task<Employee> AddAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        await _context.Employees.AddAsync(employee, cancellationToken);
        return employee;
    }

    public Task UpdateAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        _context.Employees.Update(employee);
        return Task.CompletedTask;
    }

    public async Task<EmployeeImportBatch> AddImportBatchAsync(EmployeeImportBatch batch, CancellationToken cancellationToken = default)
    {
        await _context.EmployeeImportBatches.AddAsync(batch, cancellationToken);
        return batch;
    }

    public Task UpdateImportBatchAsync(EmployeeImportBatch batch, CancellationToken cancellationToken = default)
    {
        _context.EmployeeImportBatches.Update(batch);
        return Task.CompletedTask;
    }

    public async Task AddImportBatchRowAsync(EmployeeImportBatchRow row, CancellationToken cancellationToken = default)
    {
        await _context.EmployeeImportBatchRows.AddAsync(row, cancellationToken);
    }

    public async Task<IReadOnlyList<EmployeeImportBatch>> GetImportBatchesAsync(CancellationToken cancellationToken = default)
        => await _context.EmployeeImportBatches.OrderByDescending(b => b.CreatedAt).ToListAsync(cancellationToken);

    public async Task<EmployeeImportBatch?> GetImportBatchByIdAsync(Guid batchId, CancellationToken cancellationToken = default)
        => await _context.EmployeeImportBatches
            .Include(b => b.Rows)
            .FirstOrDefaultAsync(b => b.Id == batchId, cancellationToken);

    private static IQueryable<Employee> ApplySort(IQueryable<Employee> query, string? sortBy, string? sortDir)
    {
        var desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        return (sortBy ?? "createdat").Trim().ToLowerInvariant() switch
        {
            "name" => desc ? query.OrderByDescending(e => e.Name) : query.OrderBy(e => e.Name),
            "staffid" => desc ? query.OrderByDescending(e => e.StaffId) : query.OrderBy(e => e.StaffId),
            "role" => desc ? query.OrderByDescending(e => e.Role) : query.OrderBy(e => e.Role),
            "email" => desc ? query.OrderByDescending(e => e.Email) : query.OrderBy(e => e.Email),
            _ => desc ? query.OrderByDescending(e => e.CreatedAt) : query.OrderBy(e => e.CreatedAt)
        };
    }
}
