using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IStudentSubjectAttendanceRepository
{
    Task<IReadOnlyList<StudentSubjectAttendance>> GetForDateAsync(
        Guid classId, Guid sectionId, Guid? subjectId, DateTime date, CancellationToken cancellationToken = default);

    Task UpsertBatchAsync(IEnumerable<StudentSubjectAttendance> entries, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StudentSubjectAttendance>> GetRangeAsync(
        Guid classId, Guid sectionId, Guid? subjectId, DateTime fromDate, DateTime toDate,
        CancellationToken cancellationToken = default);
}
