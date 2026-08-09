using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IExamScheduleRepository
{
    Task<IReadOnlyList<ExamSchedule>> GetFilteredAsync(Guid? classId, Guid? sectionId, CancellationToken cancellationToken = default);
    Task<ExamSchedule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsUniqueAsync(Guid examId, Guid classId, Guid sectionId, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<ExamSchedule> AddAsync(ExamSchedule entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(ExamSchedule entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(ExamSchedule entity, CancellationToken cancellationToken = default);
    Task ReplaceSubjectsAsync(Guid scheduleId, IEnumerable<ExamScheduleSubject> subjects, CancellationToken cancellationToken = default);
}
