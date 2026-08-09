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

    /// <summary>
    /// Finds the exam-schedule subject a biometric punch falls into for a class/section on a given date,
    /// matching within [StartingTime - graceBefore, EndingTime + graceAfter]. When multiple subjects match,
    /// the one whose StartingTime is closest to the punch time wins.
    /// </summary>
    Task<ExamScheduleSubject?> FindExamSubjectForPunchAsync(
        Guid classId, Guid sectionId, DateTime date, TimeSpan time,
        int graceBeforeMinutes, int graceAfterMinutes, CancellationToken cancellationToken = default);
}
