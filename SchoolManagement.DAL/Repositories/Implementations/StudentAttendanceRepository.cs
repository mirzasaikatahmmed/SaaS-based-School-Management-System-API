using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class StudentAttendanceRepository(TenantDbContext context) : IStudentAttendanceRepository
{
    public async Task<IReadOnlyList<StudentAttendance>> GetForDateAsync(
        Guid classId, Guid sectionId, DateTime date, CancellationToken cancellationToken = default)
        => await context.StudentAttendances
            .Include(a => a.Student)
            .Where(a => a.ClassId == classId && a.SectionId == sectionId && a.AttendanceDate.Date == date.Date)
            .ToListAsync(cancellationToken);

    public async Task UpsertBatchAsync(IEnumerable<StudentAttendance> entries, CancellationToken cancellationToken = default)
    {
        foreach (var entry in entries)
        {
            var existing = await context.StudentAttendances.FirstOrDefaultAsync(a =>
                a.StudentId == entry.StudentId &&
                a.ClassId == entry.ClassId &&
                a.SectionId == entry.SectionId &&
                a.AttendanceDate.Date == entry.AttendanceDate.Date, cancellationToken);

            if (existing is null)
            {
                await context.StudentAttendances.AddAsync(entry, cancellationToken);
                continue;
            }

            existing.Status = entry.Status;
            existing.Remarks = entry.Remarks;
            existing.CreatedBy = entry.CreatedBy ?? existing.CreatedBy;
            existing.UpdatedAt = DateTime.UtcNow;
            context.StudentAttendances.Update(existing);
        }
    }

    public async Task<IReadOnlyList<StudentAttendance>> GetReportAsync(
        Guid? classId, Guid? sectionId, Guid? studentId, DateTime fromDate, DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        var q = context.StudentAttendances
            .Include(a => a.Student)
            .Include(a => a.Class)
            .Include(a => a.Section)
            .Where(a => a.AttendanceDate.Date >= fromDate.Date && a.AttendanceDate.Date <= toDate.Date);

        if (classId.HasValue) q = q.Where(a => a.ClassId == classId.Value);
        if (sectionId.HasValue) q = q.Where(a => a.SectionId == sectionId.Value);
        if (studentId.HasValue) q = q.Where(a => a.StudentId == studentId.Value);

        return await q.OrderBy(a => a.AttendanceDate).ThenBy(a => a.Student.Roll).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Student>> GetActiveStudentsAsync(
        Guid classId, Guid sectionId, CancellationToken cancellationToken = default)
        => await context.Students
            .Where(s => s.IsActive && s.ClassId == classId && s.SectionId == sectionId)
            .OrderBy(s => s.Roll).ThenBy(s => s.RegisterNo)
            .ToListAsync(cancellationToken);
}
