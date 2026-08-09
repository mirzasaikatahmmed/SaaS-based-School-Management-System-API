using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class ClassControlRepository(TenantDbContext context) : IClassControlRepository
{
    public async Task<IReadOnlyList<ClassEntity>> GetAllWithSectionsAsync(CancellationToken cancellationToken = default)
        => await context.Classes
            .Include(c => c.ClassSections).ThenInclude(cs => cs.Section)
            .OrderBy(c => c.NumericName).ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);

    public async Task<ClassEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Classes.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<ClassEntity?> GetByIdWithSectionsAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Classes
            .Include(c => c.ClassSections).ThenInclude(cs => cs.Section)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var n = name.Trim().ToUpperInvariant();
        var q = context.Classes.Where(c => c.Name.ToUpper() == n);
        if (excludeId.HasValue) q = q.Where(c => c.Id != excludeId.Value);
        return await q.AnyAsync(cancellationToken);
    }

    public async Task<int> CountStudentsAsync(Guid classId, CancellationToken cancellationToken = default)
        => await context.Students.CountAsync(s => s.ClassId == classId, cancellationToken);

    public async Task<ClassEntity> AddAsync(ClassEntity entity, CancellationToken cancellationToken = default)
    {
        await context.Classes.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(ClassEntity entity, CancellationToken cancellationToken = default)
    {
        context.Classes.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ClassEntity entity, CancellationToken cancellationToken = default)
    {
        context.Classes.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<Section>> GetSectionsByIdsAsync(IEnumerable<Guid> sectionIds, CancellationToken cancellationToken = default)
        => await context.Sections.Where(s => sectionIds.Contains(s.Id)).ToListAsync(cancellationToken);

    public async Task ReplaceClassSectionsAsync(Guid classId, IEnumerable<Guid> sectionIds, CancellationToken cancellationToken = default)
    {
        var existing = await context.ClassSections.Where(cs => cs.ClassId == classId).ToListAsync(cancellationToken);
        context.ClassSections.RemoveRange(existing);
        foreach (var sectionId in sectionIds.Distinct())
        {
            await context.ClassSections.AddAsync(new ClassSection { ClassId = classId, SectionId = sectionId }, cancellationToken);
        }
    }
}
