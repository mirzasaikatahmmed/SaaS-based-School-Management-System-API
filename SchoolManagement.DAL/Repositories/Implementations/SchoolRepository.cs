using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Master;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class SchoolRepository : ISchoolRepository
{
    private readonly MasterDbContext _context;

    public SchoolRepository(MasterDbContext context)
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

    public async Task<(IReadOnlyList<Tenant> Items, int TotalCount)> SearchAsync(
        SchoolSearchFilter filter,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Tenants.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLowerInvariant();
            query = query.Where(t =>
                t.Name.ToLower().Contains(term) ||
                t.Slug.ToLower().Contains(term) ||
                (t.City != null && t.City.ToLower().Contains(term)) ||
                (t.State != null && t.State.ToLower().Contains(term)) ||
                (t.SchoolType != null && t.SchoolType.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(t => t.Name.ToLower().Contains(filter.Name.Trim().ToLowerInvariant()));

        if (!string.IsNullOrWhiteSpace(filter.Slug))
            query = query.Where(t => t.Slug == filter.Slug.Trim().ToLowerInvariant());

        if (!string.IsNullOrWhiteSpace(filter.City))
            query = query.Where(t => t.City != null && t.City.ToLower().Contains(filter.City.Trim().ToLowerInvariant()));

        if (!string.IsNullOrWhiteSpace(filter.State))
            query = query.Where(t => t.State != null && t.State.ToLower().Contains(filter.State.Trim().ToLowerInvariant()));

        if (!string.IsNullOrWhiteSpace(filter.SchoolType))
            query = query.Where(t => t.SchoolType != null && t.SchoolType.ToLower() == filter.SchoolType.Trim().ToLowerInvariant());

        if (filter.IsActive.HasValue)
            query = query.Where(t => t.IsActive == filter.IsActive.Value);

        var total = await query.CountAsync(cancellationToken);

        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize is < 1 or > 200 ? 20 : filter.PageSize;

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<Tenant> AddAsync(Tenant school, CancellationToken cancellationToken = default)
    {
        await _context.Tenants.AddAsync(school, cancellationToken);
        return school;
    }

    public Task UpdateAsync(Tenant school, CancellationToken cancellationToken = default)
    {
        school.UpdatedAt = DateTime.UtcNow;
        _context.Tenants.Update(school);
        return Task.CompletedTask;
    }

    public async Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants.AnyAsync(t => t.Slug == slug.ToLowerInvariant(), cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tenants.CountAsync(cancellationToken);
    }
}
