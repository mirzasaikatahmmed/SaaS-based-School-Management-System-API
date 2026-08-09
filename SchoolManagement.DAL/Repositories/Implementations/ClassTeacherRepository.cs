using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class ClassTeacherRepository(TenantDbContext context) : IClassTeacherRepository
{
    public async Task<IReadOnlyList<ClassTeacherAllocation>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.ClassTeacherAllocations
            .Include(a => a.Class)
            .Include(a => a.Section)
            .Include(a => a.Employee)
            .OrderBy(a => a.Class.NumericName).ThenBy(a => a.Section.Name)
            .ToListAsync(cancellationToken);

    public async Task<ClassTeacherAllocation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.ClassTeacherAllocations
            .Include(a => a.Class)
            .Include(a => a.Section)
            .Include(a => a.Employee)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<ClassTeacherAllocation?> GetByClassSectionAsync(Guid classId, Guid sectionId, CancellationToken cancellationToken = default)
        => await context.ClassTeacherAllocations
            .Include(a => a.Class)
            .Include(a => a.Section)
            .Include(a => a.Employee)
            .FirstOrDefaultAsync(a => a.ClassId == classId && a.SectionId == sectionId, cancellationToken);

    public async Task<ClassTeacherAllocation> AddAsync(ClassTeacherAllocation entity, CancellationToken cancellationToken = default)
    {
        await context.ClassTeacherAllocations.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(ClassTeacherAllocation entity, CancellationToken cancellationToken = default)
    {
        context.ClassTeacherAllocations.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ClassTeacherAllocation entity, CancellationToken cancellationToken = default)
    {
        context.ClassTeacherAllocations.Remove(entity);
        return Task.CompletedTask;
    }
}
