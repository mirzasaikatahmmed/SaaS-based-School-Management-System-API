using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public static class OfflinePaymentStatuses
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
}

public class OfflinePaymentFilter
{
    public string? Status { get; set; }
    public Guid? StudentId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public interface IOfflinePaymentRepository
{
    Task<bool> TrxIdExistsAsync(string trxId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<OfflinePayment> Items, int TotalCount)> GetFilteredAsync(OfflinePaymentFilter filter, CancellationToken cancellationToken = default);
    Task<OfflinePayment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OfflinePayment> AddAsync(OfflinePayment entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(OfflinePayment entity, CancellationToken cancellationToken = default);
}
