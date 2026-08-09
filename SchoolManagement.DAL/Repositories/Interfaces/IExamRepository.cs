using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IExamRepository
{
    Task<IReadOnlyList<Exam>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Exam?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<int> CountSchedulesAsync(Guid examId, CancellationToken cancellationToken = default);
    Task<int> CountMarkEntriesAsync(Guid examId, CancellationToken cancellationToken = default);
    Task<Exam> AddAsync(Exam entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Exam entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Exam entity, CancellationToken cancellationToken = default);
    Task ReplaceMarkDistributionsAsync(Guid examId, IEnumerable<ExamMarkDistribution> distributions, CancellationToken cancellationToken = default);
}
