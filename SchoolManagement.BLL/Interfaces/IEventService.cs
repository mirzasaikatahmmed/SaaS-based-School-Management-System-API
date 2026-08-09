using SchoolManagement.BLL.DTOs.Events;

namespace SchoolManagement.BLL.Interfaces;

public interface IEventService
{
    Task<EventListResponseDto> GetListAsync(EventFilterDto filter, CancellationToken cancellationToken = default);
    Task<EventDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EventDetailDto> CreateAsync(CreateEventDto dto, CancellationToken cancellationToken = default);
    Task<EventDetailDto> UpdateAsync(Guid id, UpdateEventDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EventDetailDto> UploadImageAsync(Guid id, Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task<EventDetailDto> TogglePublishAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EventDetailDto> ToggleShowWebsiteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PublicEventDto>> GetPublicAsync(CancellationToken cancellationToken = default);
}
