using SchoolManagement.BLL.DTOs.StudentAccounting;

namespace SchoolManagement.BLL.Interfaces;

public interface IFeesReminderService
{
    Task<IReadOnlyList<FeesReminderResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<FeesReminderResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<FeesReminderResponseDto> CreateAsync(CreateFeesReminderDto dto, CancellationToken cancellationToken = default);
    Task<FeesReminderResponseDto> UpdateAsync(Guid id, UpdateFeesReminderDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
