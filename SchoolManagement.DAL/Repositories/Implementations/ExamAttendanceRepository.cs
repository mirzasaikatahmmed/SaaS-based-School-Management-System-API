using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class ExamAttendanceRepository(TenantDbContext context) : IExamAttendanceRepository
{
    public async Task<IReadOnlyList<ExamAttendance>> GetForFilterAsync(
        Guid examId, Guid classId, Guid sectionId, Guid subjectId, CancellationToken cancellationToken = default)
        => await context.ExamAttendances
            .Include(a => a.Student)
            .Where(a => a.ExamId == examId && a.ClassId == classId && a.SectionId == sectionId && a.SubjectId == subjectId)
            .ToListAsync(cancellationToken);

    public async Task UpsertBatchAsync(IEnumerable<ExamAttendance> entries, CancellationToken cancellationToken = default)
    {
        foreach (var entry in entries)
        {
            var existing = await context.ExamAttendances.FirstOrDefaultAsync(a =>
                a.ExamId == entry.ExamId &&
                a.ClassId == entry.ClassId &&
                a.SectionId == entry.SectionId &&
                a.SubjectId == entry.SubjectId &&
                a.StudentId == entry.StudentId, cancellationToken);

            if (existing is null)
            {
                await context.ExamAttendances.AddAsync(entry, cancellationToken);
                continue;
            }

            existing.Status = entry.Status;
            existing.Remarks = entry.Remarks;
            existing.UpdatedAt = DateTime.UtcNow;
            context.ExamAttendances.Update(existing);
        }
    }

    public async Task<IReadOnlyList<Student>> GetActiveStudentsAsync(
        Guid classId, Guid sectionId, CancellationToken cancellationToken = default)
        => await context.Students
            .Where(s => s.IsActive && s.ClassId == classId && s.SectionId == sectionId)
            .OrderBy(s => s.Roll).ThenBy(s => s.RegisterNo)
            .ToListAsync(cancellationToken);
}
