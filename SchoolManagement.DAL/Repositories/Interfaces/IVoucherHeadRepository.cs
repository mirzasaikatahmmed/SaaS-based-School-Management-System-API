using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public static class VoucherHeadTypes
{
    public const string Income = "Income";
    public const string Expense = "Expense";
}

public interface IVoucherHeadRepository
{
    Task<IReadOnlyList<VoucherHead>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VoucherHead>> GetByTypeAsync(string type, CancellationToken cancellationToken = default);
    Task<VoucherHead?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<VoucherHead> AddAsync(VoucherHead entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(VoucherHead entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(VoucherHead entity, CancellationToken cancellationToken = default);
}
