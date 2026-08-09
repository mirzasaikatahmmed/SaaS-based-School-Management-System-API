using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IMarkEntryRepository
{
    Task<IReadOnlyList<MarkEntry>> GetForFilterAsync(Guid examId, Guid classId, Guid sectionId, Guid subjectId, CancellationToken cancellationToken = default);
    Task UpsertBatchAsync(IEnumerable<MarkEntry> entries, CancellationToken cancellationToken = default);
    Task<ExamScheduleSubject?> GetScheduleSubjectAsync(Guid examId, Guid classId, Guid sectionId, Guid subjectId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Student>> GetActiveStudentsByClassSectionAsync(Guid classId, Guid sectionId, CancellationToken cancellationToken = default);
}
