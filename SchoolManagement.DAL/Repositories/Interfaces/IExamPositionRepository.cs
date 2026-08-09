using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IExamPositionRepository
{
    Task<IReadOnlyList<ExamPosition>> GetByFilterAsync(
        Guid examId, Guid classId, Guid sectionId, int academicYear, CancellationToken cancellationToken = default);

    Task<ExamPosition?> GetOneAsync(
        Guid examId, Guid classId, Guid sectionId, Guid studentId, CancellationToken cancellationToken = default);

    Task UpsertRangeAsync(IEnumerable<ExamPosition> positions, CancellationToken cancellationToken = default);

    /// <summary>Sum of mark_entries.total_mark grouped by student for the given exam/class/section.</summary>
    Task<Dictionary<Guid, decimal>> GetMarkTotalsAsync(
        Guid examId, Guid classId, Guid sectionId, CancellationToken cancellationToken = default);

    /// <summary>Sum of exam_schedule_subjects.written_full_mark for the exam's schedule matching class/section.</summary>
    Task<decimal> GetFullMarksAsync(
        Guid examId, Guid classId, Guid sectionId, CancellationToken cancellationToken = default);
}
