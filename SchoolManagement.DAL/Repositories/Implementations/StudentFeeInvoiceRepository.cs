using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class StudentFeeInvoiceRepository(TenantDbContext context) : IStudentFeeInvoiceRepository
{
    private IQueryable<StudentFeeInvoice> BaseQuery() => context.StudentFeeInvoices
        .Include(i => i.Student)
        .Include(i => i.Class)
        .Include(i => i.Section)
        .Include(i => i.FeesGroup).ThenInclude(g => g.Items).ThenInclude(gi => gi.FeesType)
        .Include(i => i.FeesAllocation);

    public async Task<(IReadOnlyList<StudentFeeInvoice> Items, int TotalCount)> GetFilteredAsync(StudentFeeInvoiceFilter filter, CancellationToken cancellationToken = default)
    {
        var q = BaseQuery().AsQueryable();
        if (filter.ClassId.HasValue) q = q.Where(x => x.ClassId == filter.ClassId.Value);
        if (filter.SectionId.HasValue) q = q.Where(x => x.SectionId == filter.SectionId.Value);
        if (filter.StudentId.HasValue) q = q.Where(x => x.StudentId == filter.StudentId.Value);
        if (filter.FeesAllocationId.HasValue) q = q.Where(x => x.FeesAllocationId == filter.FeesAllocationId.Value);
        if (!string.IsNullOrWhiteSpace(filter.Status)) q = q.Where(x => x.Status == filter.Status);

        var total = await q.CountAsync(cancellationToken);
        var page = filter.Page < 1 ? 1 : filter.Page;
        var size = filter.PageSize is < 1 or > 500 ? 25 : filter.PageSize;
        var items = await q.OrderByDescending(x => x.CreatedAt).Skip((page - 1) * size).Take(size).ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task<(IReadOnlyList<StudentFeeInvoice> Items, int TotalCount)> GetDueAsync(DueInvoiceFilter filter, CancellationToken cancellationToken = default)
    {
        var q = BaseQuery().Where(x => x.Status != FeeInvoiceStatuses.Paid);
        if (filter.ClassId.HasValue) q = q.Where(x => x.ClassId == filter.ClassId.Value);
        if (filter.SectionId.HasValue) q = q.Where(x => x.SectionId == filter.SectionId.Value);
        if (filter.FeesTypeId.HasValue)
            q = q.Where(x => x.FeesGroup.Items.Any(gi => gi.FeesTypeId == filter.FeesTypeId.Value));
        if (filter.OverdueOnly == true)
        {
            var today = DateTime.UtcNow.Date;
            q = q.Where(x => x.FeesGroup.Items.Any(gi => gi.DueDate.Date < today));
        }

        var total = await q.CountAsync(cancellationToken);
        var page = filter.Page < 1 ? 1 : filter.Page;
        var size = filter.PageSize is < 1 or > 500 ? 25 : filter.PageSize;
        var items = await q.OrderByDescending(x => x.CreatedAt).Skip((page - 1) * size).Take(size).ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task<IReadOnlyList<StudentFeeInvoice>> GetByStudentAsync(Guid studentId, CancellationToken cancellationToken = default)
        => await BaseQuery().Where(x => x.StudentId == studentId).OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);

    public async Task<StudentFeeInvoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await BaseQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<StudentFeeInvoice?> GetByStudentAndAllocationAsync(Guid studentId, Guid allocationId, CancellationToken cancellationToken = default)
        => await context.StudentFeeInvoices.FirstOrDefaultAsync(x => x.StudentId == studentId && x.FeesAllocationId == allocationId, cancellationToken);

    public async Task<StudentFeeInvoice> AddAsync(StudentFeeInvoice entity, CancellationToken cancellationToken = default)
    {
        await context.StudentFeeInvoices.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(StudentFeeInvoice entity, CancellationToken cancellationToken = default)
    {
        context.StudentFeeInvoices.Update(entity);
        return Task.CompletedTask;
    }
}
