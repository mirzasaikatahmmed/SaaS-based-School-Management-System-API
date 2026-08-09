using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Helpers;

/// <summary>
/// Resolves which students take which subjects when the class has elective groups (e.g. 4th subject).
/// </summary>
public static class ElectiveSubjectHelper
{
    /// <summary>
    /// Students who should appear for this subject: all students if subject is mandatory;
    /// only enrolled students if subject is elective; none if elective and student has no enrollment yet
    /// (caller may still show unassigned students separately).
    /// </summary>
    public static async Task<IReadOnlyList<Student>> FilterStudentsForSubjectAsync(
        IUnitOfWork uow,
        Guid classId,
        Guid sectionId,
        Guid subjectId,
        int academicYear,
        IReadOnlyList<Student> students,
        CancellationToken cancellationToken = default)
    {
        var assignment = await uow.ClassSubjectAssignments.GetByClassSectionAsync(classId, sectionId, cancellationToken);
        var item = assignment?.Items.FirstOrDefault(i => i.SubjectId == subjectId);
        if (item is null || !item.IsElective)
            return students;

        var enrolledIds = await uow.StudentSubjectEnrollments.GetStudentIdsForSubjectAsync(
            classId, sectionId, subjectId, academicYear, cancellationToken);
        return students.Where(s => enrolledIds.Contains(s.Id)).ToList();
    }

    /// <summary>
    /// Subjects that should appear on a student's report card / progress sheet.
    /// </summary>
    public static async Task<IReadOnlyList<ExamScheduleSubject>> FilterScheduleSubjectsForStudentAsync(
        IUnitOfWork uow,
        Guid classId,
        Guid sectionId,
        Guid studentId,
        int academicYear,
        IReadOnlyList<ExamScheduleSubject> scheduleSubjects,
        CancellationToken cancellationToken = default)
    {
        var assignment = await uow.ClassSubjectAssignments.GetByClassSectionAsync(classId, sectionId, cancellationToken);
        if (assignment is null)
            return scheduleSubjects;

        var electiveBySubject = assignment.Items
            .Where(i => i.IsElective)
            .ToDictionary(i => i.SubjectId, i => i.ElectiveGroup ?? ElectiveGroups.Fourth);

        if (electiveBySubject.Count == 0)
            return scheduleSubjects;

        var enrollments = await uow.StudentSubjectEnrollments.GetForStudentYearAsync(studentId, academicYear, cancellationToken);
        var enrolledSubjectIds = enrollments.Select(e => e.SubjectId).ToHashSet();

        return scheduleSubjects.Where(ss =>
        {
            if (!electiveBySubject.TryGetValue(ss.SubjectId, out _))
                return true; // mandatory
            return enrolledSubjectIds.Contains(ss.SubjectId);
        }).ToList();
    }
}
