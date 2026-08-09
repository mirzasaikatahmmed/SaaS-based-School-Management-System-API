using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IExamTermRepository
{
    Task<IReadOnlyList<ExamTerm>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ExamTerm?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<int> CountExamsUsingAsync(Guid termId, CancellationToken cancellationToken = default);
    Task<ExamTerm> AddAsync(ExamTerm entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(ExamTerm entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(ExamTerm entity, CancellationToken cancellationToken = default);
}
