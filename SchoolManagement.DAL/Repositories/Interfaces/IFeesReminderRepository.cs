using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IFeesReminderRepository
{
    Task<IReadOnlyList<FeesReminder>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<FeesReminder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<FeesReminder> AddAsync(FeesReminder entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(FeesReminder entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(FeesReminder entity, CancellationToken cancellationToken = default);
}
