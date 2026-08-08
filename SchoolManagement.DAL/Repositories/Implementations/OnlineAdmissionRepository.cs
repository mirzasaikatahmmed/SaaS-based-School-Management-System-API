using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class OnlineAdmissionRepository : IOnlineAdmissionRepository
{
    private readonly TenantDbContext _context;

    public OnlineAdmissionRepository(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<OnlineAdmission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.OnlineAdmissions
            .Include(o => o.Class)
            .Include(o => o.Student)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<OnlineAdmission?> GetByReferenceNoAsync(string referenceNo, CancellationToken cancellationToken = default)
    {
        return await _context.OnlineAdmissions
            .Include(o => o.Class)
            .FirstOrDefaultAsync(o => o.ReferenceNo == referenceNo, cancellationToken);
    }

    public async Task<(IReadOnlyList<OnlineAdmission> Items, int TotalCount)> SearchAsync(
        OnlineAdmissionSearchFilter filter,
        CancellationToken cancellationToken = default)
    {
        var query = _context.OnlineAdmissions.AsQueryable();

        if (filter.ClassId.HasValue)
            query = query.Where(o => o.ClassId == filter.ClassId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Status))
            query = query.Where(o => o.Status == filter.Status.Trim());

        if (!string.IsNullOrWhiteSpace(filter.PaymentStatus))
            query = query.Where(o => o.PaymentStatus == filter.PaymentStatus.Trim());

        if (filter.AcademicYear.HasValue)
            query = query.Where(o => o.AcademicYear == filter.AcademicYear.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLowerInvariant();
            query = query.Where(o =>
                o.ReferenceNo.ToLower().Contains(term) ||
                o.MobileNo.Contains(term) ||
                o.FirstName.ToLower().Contains(term) ||
                (o.LastName != null && o.LastName.ToLower().Contains(term)) ||
                ((o.FirstName + " " + (o.LastName ?? "")).ToLower().Contains(term)));
        }

        var total = await query.CountAsync(cancellationToken);
        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize is < 1 or > 200 ? 10 : filter.PageSize;

        var items = await query
            .OrderByDescending(o => o.ApplyDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<OnlineAdmission> AddAsync(OnlineAdmission entity, CancellationToken cancellationToken = default)
    {
        await _context.OnlineAdmissions.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(OnlineAdmission entity, CancellationToken cancellationToken = default)
    {
        _context.OnlineAdmissions.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(OnlineAdmission entity, CancellationToken cancellationToken = default)
    {
        _context.OnlineAdmissions.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> ReferenceNoExistsAsync(string referenceNo, CancellationToken cancellationToken = default)
    {
        return await _context.OnlineAdmissions.AnyAsync(o => o.ReferenceNo == referenceNo, cancellationToken);
    }
}
