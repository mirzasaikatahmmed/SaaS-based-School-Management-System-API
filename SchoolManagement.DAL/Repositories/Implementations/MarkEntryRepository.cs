using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class MarkEntryRepository(TenantDbContext context) : IMarkEntryRepository
{
    public async Task<IReadOnlyList<MarkEntry>> GetForFilterAsync(
        Guid examId, Guid classId, Guid sectionId, Guid subjectId, CancellationToken cancellationToken = default)
        => await context.MarkEntries
            .Include(m => m.Student).ThenInclude(s => s.Category)
            .Where(m => m.ExamId == examId && m.ClassId == classId && m.SectionId == sectionId && m.SubjectId == subjectId)
            .ToListAsync(cancellationToken);

    public async Task UpsertBatchAsync(IEnumerable<MarkEntry> entries, CancellationToken cancellationToken = default)
    {
        foreach (var entry in entries)
        {
            var existing = await context.MarkEntries.FirstOrDefaultAsync(m =>
                m.ExamId == entry.ExamId &&
                m.ClassId == entry.ClassId &&
                m.SectionId == entry.SectionId &&
                m.SubjectId == entry.SubjectId &&
                m.StudentId == entry.StudentId, cancellationToken);

            if (existing is null)
            {
                await context.MarkEntries.AddAsync(entry, cancellationToken);
                continue;
            }

            existing.IsAbsent = entry.IsAbsent;
            existing.WrittenMark = entry.WrittenMark;
            existing.McqMark = entry.McqMark;
            existing.TotalMark = entry.TotalMark;
            existing.UpdatedAt = DateTime.UtcNow;
            context.MarkEntries.Update(existing);
        }
    }

    public async Task<ExamScheduleSubject?> GetScheduleSubjectAsync(
        Guid examId, Guid classId, Guid sectionId, Guid subjectId, CancellationToken cancellationToken = default)
        => await context.ExamScheduleSubjects
            .Include(s => s.Schedule)
            .Where(s => s.Schedule.ExamId == examId &&
                        s.Schedule.ClassId == classId &&
                        s.Schedule.SectionId == sectionId &&
                        s.SubjectId == subjectId)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<Student>> GetActiveStudentsByClassSectionAsync(
        Guid classId, Guid sectionId, CancellationToken cancellationToken = default)
        => await context.Students
            .Include(s => s.Category)
            .Where(s => s.IsActive && s.ClassId == classId && s.SectionId == sectionId)
            .OrderBy(s => s.Roll).ThenBy(s => s.RegisterNo)
            .ToListAsync(cancellationToken);
}
