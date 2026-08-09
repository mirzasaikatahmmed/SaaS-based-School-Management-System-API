using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class OfflinePaymentRepository(TenantDbContext context) : IOfflinePaymentRepository
{
    public async Task<bool> TrxIdExistsAsync(string trxId, CancellationToken cancellationToken = default)
        => await context.OfflinePayments.AnyAsync(x => x.TrxId == trxId, cancellationToken);

    public async Task<(IReadOnlyList<OfflinePayment> Items, int TotalCount)> GetFilteredAsync(OfflinePaymentFilter filter, CancellationToken cancellationToken = default)
    {
        var q = context.OfflinePayments
            .Include(x => x.Student)
            .Include(x => x.PaymentType)
            .Include(x => x.Class)
            .Include(x => x.Section)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Status)) q = q.Where(x => x.Status == filter.Status);
        if (filter.StudentId.HasValue) q = q.Where(x => x.StudentId == filter.StudentId.Value);

        var total = await q.CountAsync(cancellationToken);
        var page = filter.Page < 1 ? 1 : filter.Page;
        var size = filter.PageSize is < 1 or > 500 ? 25 : filter.PageSize;
        var items = await q.OrderByDescending(x => x.SubmitDate).Skip((page - 1) * size).Take(size).ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task<OfflinePayment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.OfflinePayments
            .Include(x => x.Student)
            .Include(x => x.PaymentType)
            .Include(x => x.Class)
            .Include(x => x.Section)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<OfflinePayment> AddAsync(OfflinePayment entity, CancellationToken cancellationToken = default)
    {
        await context.OfflinePayments.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(OfflinePayment entity, CancellationToken cancellationToken = default)
    {
        context.OfflinePayments.Update(entity);
        return Task.CompletedTask;
    }
}
