using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class StudentSubjectEnrollmentRepository(TenantDbContext context) : IStudentSubjectEnrollmentRepository
{
    public async Task<IReadOnlyList<StudentSubjectEnrollment>> GetForClassAsync(
        Guid classId, Guid sectionId, int academicYear, string? electiveGroup = null,
        CancellationToken cancellationToken = default)
    {
        var q = context.StudentSubjectEnrollments
            .Include(e => e.Student)
            .Include(e => e.Subject)
            .Include(e => e.AdditionalSubject)
            .Where(e => e.ClassId == classId && e.SectionId == sectionId && e.AcademicYear == academicYear);
        if (!string.IsNullOrWhiteSpace(electiveGroup))
            q = q.Where(e => e.ElectiveGroup == electiveGroup.Trim());
        return await q.OrderBy(e => e.Student.Roll).ThenBy(e => e.Student.RegisterNo).ToListAsync(cancellationToken);
    }

    public async Task<StudentSubjectEnrollment?> GetForStudentAsync(
        Guid studentId, int academicYear, string electiveGroup, CancellationToken cancellationToken = default)
        => await context.StudentSubjectEnrollments
            .Include(e => e.Subject)
            .Include(e => e.AdditionalSubject)
            .FirstOrDefaultAsync(e =>
                e.StudentId == studentId &&
                e.AcademicYear == academicYear &&
                e.ElectiveGroup == electiveGroup.Trim(), cancellationToken);

    public async Task<IReadOnlyList<StudentSubjectEnrollment>> GetForStudentYearAsync(
        Guid studentId, int academicYear, CancellationToken cancellationToken = default)
        => await context.StudentSubjectEnrollments
            .Include(e => e.Subject)
            .Include(e => e.AdditionalSubject)
            .Where(e => e.StudentId == studentId && e.AcademicYear == academicYear)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlySet<Guid>> GetStudentIdsForSubjectAsync(
        Guid classId, Guid sectionId, Guid subjectId, int academicYear, CancellationToken cancellationToken = default)
    {
        var ids = await context.StudentSubjectEnrollments
            .Where(e => e.ClassId == classId && e.SectionId == sectionId
                        && e.SubjectId == subjectId && e.AcademicYear == academicYear)
            .Select(e => e.StudentId)
            .ToListAsync(cancellationToken);
        return ids.ToHashSet();
    }

    public async Task UpsertAsync(StudentSubjectEnrollment enrollment, CancellationToken cancellationToken = default)
    {
        var existing = await context.StudentSubjectEnrollments.FirstOrDefaultAsync(e =>
            e.StudentId == enrollment.StudentId &&
            e.AcademicYear == enrollment.AcademicYear &&
            e.ElectiveGroup == enrollment.ElectiveGroup, cancellationToken);

        if (existing is null)
        {
            await context.StudentSubjectEnrollments.AddAsync(enrollment, cancellationToken);
            return;
        }

        existing.SubjectId = enrollment.SubjectId;
        existing.ClassId = enrollment.ClassId;
        existing.SectionId = enrollment.SectionId;
        existing.AdditionalSubjectId = enrollment.AdditionalSubjectId;
        existing.UpdatedAt = DateTime.UtcNow;
        context.StudentSubjectEnrollments.Update(existing);
    }

    public async Task UpsertRangeAsync(IEnumerable<StudentSubjectEnrollment> enrollments, CancellationToken cancellationToken = default)
    {
        foreach (var enrollment in enrollments)
            await UpsertAsync(enrollment, cancellationToken);
    }
}
