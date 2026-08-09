using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class ClassSubjectAssignmentRepository(TenantDbContext context) : IClassSubjectAssignmentRepository
{
    public async Task<IReadOnlyList<ClassSubjectAssignment>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.ClassSubjectAssignments
            .Include(a => a.Class)
            .Include(a => a.Section)
            .Include(a => a.Items).ThenInclude(i => i.Subject)
            .OrderBy(a => a.Class.NumericName).ThenBy(a => a.Section.Name)
            .ToListAsync(cancellationToken);

    public async Task<ClassSubjectAssignment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.ClassSubjectAssignments
            .Include(a => a.Class)
            .Include(a => a.Section)
            .Include(a => a.Items).ThenInclude(i => i.Subject)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<ClassSubjectAssignment?> GetByClassSectionAsync(Guid classId, Guid sectionId, CancellationToken cancellationToken = default)
        => await context.ClassSubjectAssignments
            .Include(a => a.Class)
            .Include(a => a.Section)
            .Include(a => a.Items).ThenInclude(i => i.Subject)
            .FirstOrDefaultAsync(a => a.ClassId == classId && a.SectionId == sectionId, cancellationToken);

    public async Task<ClassSubjectAssignment> AddAsync(ClassSubjectAssignment entity, CancellationToken cancellationToken = default)
    {
        await context.ClassSubjectAssignments.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(ClassSubjectAssignment entity, CancellationToken cancellationToken = default)
    {
        context.ClassSubjectAssignments.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ClassSubjectAssignment entity, CancellationToken cancellationToken = default)
    {
        context.ClassSubjectAssignments.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task ReplaceItemsAsync(Guid assignmentId, IEnumerable<ClassSubjectAssignmentItem> items, CancellationToken cancellationToken = default)
    {
        var existing = await context.ClassSubjectAssignmentItems.Where(i => i.AssignmentId == assignmentId).ToListAsync(cancellationToken);
        context.ClassSubjectAssignmentItems.RemoveRange(existing);
        await context.ClassSubjectAssignmentItems.AddRangeAsync(items, cancellationToken);
    }
}
