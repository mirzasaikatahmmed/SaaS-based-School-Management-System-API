using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public class EventSearchFilter
{
    public string? Search { get; set; }
    public Guid? EventTypeId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool? IsActive { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public interface IEventRepository
{
    Task<(IReadOnlyList<SchoolEvent> Items, int TotalCount)> SearchAsync(EventSearchFilter filter, CancellationToken cancellationToken = default);
    Task<SchoolEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SchoolEvent>> GetPublicAsync(CancellationToken cancellationToken = default);
    Task<SchoolEvent> AddAsync(SchoolEvent entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(SchoolEvent entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(SchoolEvent entity, CancellationToken cancellationToken = default);
}
