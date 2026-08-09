using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class SectionControlRepository(TenantDbContext context) : ISectionControlRepository
{
    public async Task<IReadOnlyList<Section>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.Sections.OrderBy(s => s.Name).ToListAsync(cancellationToken);

    public async Task<Section?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Sections.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var n = name.Trim().ToUpperInvariant();
        var q = context.Sections.Where(s => s.Name.ToUpper() == n);
        if (excludeId.HasValue) q = q.Where(s => s.Id != excludeId.Value);
        return await q.AnyAsync(cancellationToken);
    }

    public async Task<int> CountStudentsAsync(Guid sectionId, CancellationToken cancellationToken = default)
        => await context.Students.CountAsync(s => s.SectionId == sectionId, cancellationToken);

    public async Task<int> CountClassLinksAsync(Guid sectionId, CancellationToken cancellationToken = default)
        => await context.ClassSections.CountAsync(cs => cs.SectionId == sectionId, cancellationToken);

    public async Task<Section> AddAsync(Section entity, CancellationToken cancellationToken = default)
    {
        await context.Sections.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(Section entity, CancellationToken cancellationToken = default)
    {
        context.Sections.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Section entity, CancellationToken cancellationToken = default)
    {
        context.Sections.Remove(entity);
        return Task.CompletedTask;
    }
}
