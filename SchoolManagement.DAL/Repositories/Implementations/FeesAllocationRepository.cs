using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class FeesAllocationRepository(TenantDbContext context) : IFeesAllocationRepository
{
    public async Task<IReadOnlyList<FeesAllocation>> GetFilteredAsync(FeesAllocationFilter filter, CancellationToken cancellationToken = default)
    {
        var q = context.FeesAllocations
            .Include(a => a.Class)
            .Include(a => a.Section)
            .Include(a => a.FeesGroup).ThenInclude(g => g.Items).ThenInclude(i => i.FeesType)
            .AsQueryable();

        if (filter.ClassId.HasValue) q = q.Where(x => x.ClassId == filter.ClassId.Value);
        if (filter.SectionId.HasValue) q = q.Where(x => x.SectionId == filter.SectionId.Value);
        if (filter.AcademicYear.HasValue) q = q.Where(x => x.AcademicYear == filter.AcademicYear.Value);
        if (filter.IsActive.HasValue) q = q.Where(x => x.IsActive == filter.IsActive.Value);

        return await q.OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<FeesAllocation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.FeesAllocations
            .Include(a => a.Class)
            .Include(a => a.Section)
            .Include(a => a.FeesGroup).ThenInclude(g => g.Items).ThenInclude(i => i.FeesType)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<bool> ExistsUniqueAsync(Guid classId, Guid sectionId, Guid feesGroupId, int academicYear, Guid? excludeId = null, CancellationToken cancellationToken = default)
        => await context.FeesAllocations.AnyAsync(x =>
            x.ClassId == classId && x.SectionId == sectionId && x.FeesGroupId == feesGroupId &&
            x.AcademicYear == academicYear && (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);

    public async Task<FeesAllocation> AddAsync(FeesAllocation entity, CancellationToken cancellationToken = default)
    {
        await context.FeesAllocations.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(FeesAllocation entity, CancellationToken cancellationToken = default)
    {
        context.FeesAllocations.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(FeesAllocation entity, CancellationToken cancellationToken = default)
    {
        context.FeesAllocations.Remove(entity);
        return Task.CompletedTask;
    }
}
