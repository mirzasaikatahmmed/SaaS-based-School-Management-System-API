using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IStudentAttendanceRepository
{
    Task<IReadOnlyList<StudentAttendance>> GetForDateAsync(
        Guid classId, Guid sectionId, DateTime date, CancellationToken cancellationToken = default);

    Task UpsertBatchAsync(IEnumerable<StudentAttendance> entries, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StudentAttendance>> GetReportAsync(
        Guid? classId, Guid? sectionId, Guid? studentId, DateTime fromDate, DateTime toDate,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Student>> GetActiveStudentsAsync(
        Guid classId, Guid sectionId, CancellationToken cancellationToken = default);
}
