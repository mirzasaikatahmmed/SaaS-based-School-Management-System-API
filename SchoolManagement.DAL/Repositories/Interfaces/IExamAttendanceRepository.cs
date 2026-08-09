using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IExamAttendanceRepository
{
    Task<IReadOnlyList<ExamAttendance>> GetForFilterAsync(
        Guid examId, Guid classId, Guid sectionId, Guid subjectId, CancellationToken cancellationToken = default);

    Task UpsertBatchAsync(IEnumerable<ExamAttendance> entries, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Student>> GetActiveStudentsAsync(
        Guid classId, Guid sectionId, CancellationToken cancellationToken = default);
}
