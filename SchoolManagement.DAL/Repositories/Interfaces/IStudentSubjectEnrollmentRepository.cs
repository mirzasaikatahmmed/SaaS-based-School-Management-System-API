using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IStudentSubjectEnrollmentRepository
{
    Task<IReadOnlyList<StudentSubjectEnrollment>> GetForClassAsync(
        Guid classId, Guid sectionId, int academicYear, string? electiveGroup = null,
        CancellationToken cancellationToken = default);

    Task<StudentSubjectEnrollment?> GetForStudentAsync(
        Guid studentId, int academicYear, string electiveGroup, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StudentSubjectEnrollment>> GetForStudentYearAsync(
        Guid studentId, int academicYear, CancellationToken cancellationToken = default);

    Task<IReadOnlySet<Guid>> GetStudentIdsForSubjectAsync(
        Guid classId, Guid sectionId, Guid subjectId, int academicYear, CancellationToken cancellationToken = default);

    Task UpsertAsync(StudentSubjectEnrollment enrollment, CancellationToken cancellationToken = default);

    Task UpsertRangeAsync(IEnumerable<StudentSubjectEnrollment> enrollments, CancellationToken cancellationToken = default);
}
