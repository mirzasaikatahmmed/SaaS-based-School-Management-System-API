using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public class OnlineAdmissionSearchFilter
{
    public Guid? ClassId { get; set; }
    public string? Status { get; set; }
    public string? PaymentStatus { get; set; }
    public int? AcademicYear { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public interface IOnlineAdmissionRepository
{
    Task<OnlineAdmission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OnlineAdmission?> GetByReferenceNoAsync(string referenceNo, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<OnlineAdmission> Items, int TotalCount)> SearchAsync(
        OnlineAdmissionSearchFilter filter,
        CancellationToken cancellationToken = default);
    Task<OnlineAdmission> AddAsync(OnlineAdmission entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(OnlineAdmission entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(OnlineAdmission entity, CancellationToken cancellationToken = default);
    Task<bool> ReferenceNoExistsAsync(string referenceNo, CancellationToken cancellationToken = default);
}
