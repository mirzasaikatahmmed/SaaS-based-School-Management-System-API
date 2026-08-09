using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class SubjectRepository(TenantDbContext context) : ISubjectRepository
{
    public async Task<IReadOnlyList<Subject>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.Subjects.OrderBy(s => s.Name).ToListAsync(cancellationToken);

    public async Task<Subject?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Subjects.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var c = code.Trim().ToUpperInvariant();
        var q = context.Subjects.Where(s => s.Code.ToUpper() == c);
        if (excludeId.HasValue) q = q.Where(s => s.Id != excludeId.Value);
        return await q.AnyAsync(cancellationToken);
    }

    public async Task<int> CountAssignmentUsageAsync(Guid subjectId, CancellationToken cancellationToken = default)
        => await context.ClassSubjectAssignmentItems.CountAsync(i => i.SubjectId == subjectId, cancellationToken);

    public async Task<Subject> AddAsync(Subject entity, CancellationToken cancellationToken = default)
    {
        await context.Subjects.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(Subject entity, CancellationToken cancellationToken = default)
    {
        context.Subjects.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Subject entity, CancellationToken cancellationToken = default)
    {
        context.Subjects.Remove(entity);
        return Task.CompletedTask;
    }
}
