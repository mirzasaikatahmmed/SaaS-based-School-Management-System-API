using SchoolManagement.BLL.DTOs.Events;

namespace SchoolManagement.BLL.Interfaces;

public interface IEventTypeService
{
    Task<IReadOnlyList<EventTypeDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<EventTypeDto> CreateAsync(CreateEventTypeDto dto, CancellationToken cancellationToken = default);
    Task<EventTypeDto> UpdateAsync(Guid id, UpdateEventTypeDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
