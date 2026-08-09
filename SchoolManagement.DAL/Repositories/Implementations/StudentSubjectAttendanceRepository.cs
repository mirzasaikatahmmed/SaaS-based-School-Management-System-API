using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class StudentSubjectAttendanceRepository(TenantDbContext context) : IStudentSubjectAttendanceRepository
{
    public async Task<IReadOnlyList<StudentSubjectAttendance>> GetForDateAsync(
        Guid classId, Guid sectionId, Guid? subjectId, DateTime date, CancellationToken cancellationToken = default)
    {
        var q = context.StudentSubjectAttendances
            .Include(a => a.Student)
            .Include(a => a.Subject)
            .Where(a => a.ClassId == classId && a.SectionId == sectionId && a.AttendanceDate.Date == date.Date);
        if (subjectId.HasValue) q = q.Where(a => a.SubjectId == subjectId.Value);
        return await q.ToListAsync(cancellationToken);
    }

    public async Task UpsertBatchAsync(IEnumerable<StudentSubjectAttendance> entries, CancellationToken cancellationToken = default)
    {
        foreach (var entry in entries)
        {
            var existing = await context.StudentSubjectAttendances.FirstOrDefaultAsync(a =>
                a.StudentId == entry.StudentId &&
                a.SubjectId == entry.SubjectId &&
                a.AttendanceDate.Date == entry.AttendanceDate.Date, cancellationToken);

            if (existing is null)
            {
                await context.StudentSubjectAttendances.AddAsync(entry, cancellationToken);
                continue;
            }

            existing.Status = entry.Status;
            existing.Remarks = entry.Remarks;
            existing.ClassId = entry.ClassId;
            existing.SectionId = entry.SectionId;
            existing.CreatedBy = entry.CreatedBy ?? existing.CreatedBy;
            existing.UpdatedAt = DateTime.UtcNow;
            context.StudentSubjectAttendances.Update(existing);
        }
    }

    public async Task<IReadOnlyList<StudentSubjectAttendance>> GetRangeAsync(
        Guid classId, Guid sectionId, Guid? subjectId, DateTime fromDate, DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        var q = context.StudentSubjectAttendances
            .Include(a => a.Student)
            .Include(a => a.Subject)
            .Where(a => a.ClassId == classId && a.SectionId == sectionId
                        && a.AttendanceDate.Date >= fromDate.Date && a.AttendanceDate.Date <= toDate.Date);
        if (subjectId.HasValue) q = q.Where(a => a.SubjectId == subjectId.Value);
        return await q.OrderBy(a => a.AttendanceDate).ThenBy(a => a.Student.Roll).ToListAsync(cancellationToken);
    }
}
