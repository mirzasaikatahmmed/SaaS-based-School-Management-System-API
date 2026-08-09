using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface ISectionControlRepository
{
    Task<IReadOnlyList<Section>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Section?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<int> CountStudentsAsync(Guid sectionId, CancellationToken cancellationToken = default);
    Task<int> CountClassLinksAsync(Guid sectionId, CancellationToken cancellationToken = default);
    Task<Section> AddAsync(Section entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Section entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Section entity, CancellationToken cancellationToken = default);
}
