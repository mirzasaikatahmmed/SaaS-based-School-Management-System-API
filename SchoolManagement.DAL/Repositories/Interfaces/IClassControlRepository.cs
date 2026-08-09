using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IClassControlRepository
{
    Task<IReadOnlyList<ClassEntity>> GetAllWithSectionsAsync(CancellationToken cancellationToken = default);
    Task<ClassEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ClassEntity?> GetByIdWithSectionsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<int> CountStudentsAsync(Guid classId, CancellationToken cancellationToken = default);
    Task<ClassEntity> AddAsync(ClassEntity entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(ClassEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(ClassEntity entity, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Section>> GetSectionsByIdsAsync(IEnumerable<Guid> sectionIds, CancellationToken cancellationToken = default);
    Task ReplaceClassSectionsAsync(Guid classId, IEnumerable<Guid> sectionIds, CancellationToken cancellationToken = default);
}
