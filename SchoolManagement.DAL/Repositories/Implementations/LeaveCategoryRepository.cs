using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class LeaveCategoryRepository(TenantDbContext context) : ILeaveCategoryRepository
{
    public async Task<IReadOnlyList<LeaveCategory>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.LeaveCategories.OrderBy(c => c.Role).ThenBy(c => c.Name).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<LeaveCategory>> GetByRoleAsync(string role, CancellationToken cancellationToken = default)
    {
        var r = role.Trim().ToLowerInvariant();
        return await context.LeaveCategories
            .Where(c => c.IsActive && c.Role.ToLower() == r)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<LeaveCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.LeaveCategories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<bool> NameRoleExistsAsync(string name, string role, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var n = name.Trim().ToUpperInvariant();
        var r = role.Trim().ToUpperInvariant();
        var q = context.LeaveCategories.Where(c => c.Name.ToUpper() == n && c.Role.ToUpper() == r);
        if (excludeId.HasValue) q = q.Where(c => c.Id != excludeId.Value);
        return await q.AnyAsync(cancellationToken);
    }

    public async Task<int> CountRequestsAsync(Guid categoryId, CancellationToken cancellationToken = default)
        => await context.LeaveRequests.CountAsync(r => r.LeaveCategoryId == categoryId, cancellationToken);

    public async Task<LeaveCategory> AddAsync(LeaveCategory entity, CancellationToken cancellationToken = default)
    {
        await context.LeaveCategories.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(LeaveCategory entity, CancellationToken cancellationToken = default)
    {
        context.LeaveCategories.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(LeaveCategory entity, CancellationToken cancellationToken = default)
    {
        context.LeaveCategories.Remove(entity);
        return Task.CompletedTask;
    }
}
