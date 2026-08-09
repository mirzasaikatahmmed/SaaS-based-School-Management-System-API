using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class AcademicSessionRepository(TenantDbContext context) : IAcademicSessionRepository
{
    public async Task<IReadOnlyList<AcademicSession>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.AcademicSessions.OrderByDescending(s => s.Name).ToListAsync(cancellationToken);

    public async Task<AcademicSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.AcademicSessions.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<AcademicSession?> GetCurrentAsync(CancellationToken cancellationToken = default)
        => await context.AcademicSessions.FirstOrDefaultAsync(s => s.IsSelected, cancellationToken);

    public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var n = name.Trim().ToUpperInvariant();
        var q = context.AcademicSessions.Where(s => s.Name.ToUpper() == n);
        if (excludeId.HasValue) q = q.Where(s => s.Id != excludeId.Value);
        return await q.AnyAsync(cancellationToken);
    }

    public async Task ClearSelectedAsync(CancellationToken cancellationToken = default)
    {
        var selected = await context.AcademicSessions.Where(s => s.IsSelected).ToListAsync(cancellationToken);
        foreach (var s in selected) s.IsSelected = false;
    }

    public async Task<AcademicSession> AddAsync(AcademicSession entity, CancellationToken cancellationToken = default)
    {
        await context.AcademicSessions.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(AcademicSession entity, CancellationToken cancellationToken = default)
    {
        context.AcademicSessions.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(AcademicSession entity, CancellationToken cancellationToken = default)
    {
        context.AcademicSessions.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<int> CountStudentsForYearAsync(int academicYear, CancellationToken cancellationToken = default)
        => await context.Students.CountAsync(s => s.AcademicYear == academicYear, cancellationToken);
}
