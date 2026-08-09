using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IEventTypeRepository
{
    Task<IReadOnlyList<EventType>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<EventType?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<int> CountEventsUsingAsync(Guid eventTypeId, CancellationToken cancellationToken = default);
    Task<EventType> AddAsync(EventType entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(EventType entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(EventType entity, CancellationToken cancellationToken = default);
}
