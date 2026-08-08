using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class DeactivateReasonRepository : IDeactivateReasonRepository
{
    private readonly TenantDbContext _context;

    public DeactivateReasonRepository(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<DeactivateReason>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.DeactivateReasons
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<DeactivateReason?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.DeactivateReasons.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<DeactivateReason?> GetByReasonAsync(string reason, CancellationToken cancellationToken = default)
    {
        var normalized = reason.Trim().ToUpperInvariant();
        return await _context.DeactivateReasons
            .FirstOrDefaultAsync(r => r.Reason.ToUpper() == normalized, cancellationToken);
    }

    public async Task<bool> ReasonExistsAsync(
        string reason,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var normalized = reason.Trim().ToUpperInvariant();
        var query = _context.DeactivateReasons.Where(r => r.Reason.ToUpper() == normalized);
        if (excludeId.HasValue)
            query = query.Where(r => r.Id != excludeId.Value);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<int> CountStudentsUsingAsync(Guid reasonId, CancellationToken cancellationToken = default)
    {
        return await _context.Students.CountAsync(s => s.DeactivateReasonId == reasonId, cancellationToken);
    }

    public async Task<DeactivateReason> AddAsync(DeactivateReason entity, CancellationToken cancellationToken = default)
    {
        await _context.DeactivateReasons.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(DeactivateReason entity, CancellationToken cancellationToken = default)
    {
        _context.DeactivateReasons.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(DeactivateReason entity, CancellationToken cancellationToken = default)
    {
        _context.DeactivateReasons.Remove(entity);
        return Task.CompletedTask;
    }
}
