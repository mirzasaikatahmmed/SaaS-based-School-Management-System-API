using SchoolManagement.DAL.Entities.Master;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IGlobalSettingsRepository
{
    Task<GlobalSettings?> GetAsync(CancellationToken cancellationToken = default);
    Task<GlobalSettings> AddAsync(GlobalSettings entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(GlobalSettings entity, CancellationToken cancellationToken = default);
}
