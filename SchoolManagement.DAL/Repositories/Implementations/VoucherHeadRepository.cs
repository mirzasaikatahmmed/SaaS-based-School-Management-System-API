using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class VoucherHeadRepository(TenantDbContext context) : IVoucherHeadRepository
{
    public async Task<IReadOnlyList<VoucherHead>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.VoucherHeads.OrderBy(x => x.Name).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<VoucherHead>> GetByTypeAsync(string type, CancellationToken cancellationToken = default)
        => await context.VoucherHeads.Where(x => x.Type == type && x.IsActive).OrderBy(x => x.Name).ToListAsync(cancellationToken);

    public async Task<VoucherHead?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.VoucherHeads.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
        => await context.VoucherHeads.AnyAsync(x => x.Name.ToLower() == name.ToLower() && (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);

    public async Task<VoucherHead> AddAsync(VoucherHead entity, CancellationToken cancellationToken = default)
    {
        await context.VoucherHeads.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(VoucherHead entity, CancellationToken cancellationToken = default)
    {
        context.VoucherHeads.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(VoucherHead entity, CancellationToken cancellationToken = default)
    {
        context.VoucherHeads.Remove(entity);
        return Task.CompletedTask;
    }
}
