using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface ISchoolSettingsRepository
{
    Task<SchoolSettings?> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns the single settings row, creating (and persisting) defaults if none exist.</summary>
    Task<SchoolSettings> GetOrCreateAsync(CancellationToken cancellationToken = default);

    Task UpdateAsync(SchoolSettings entity, CancellationToken cancellationToken = default);
}
