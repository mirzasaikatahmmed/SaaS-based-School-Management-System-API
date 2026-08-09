using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class ExamPositionRepository(TenantDbContext context) : IExamPositionRepository
{
    public async Task<IReadOnlyList<ExamPosition>> GetByFilterAsync(
        Guid examId, Guid classId, Guid sectionId, int academicYear, CancellationToken cancellationToken = default)
        => await context.ExamPositions
            .Include(p => p.Student).ThenInclude(s => s.Category)
            .Include(p => p.Class)
            .Include(p => p.Section)
            .Where(p => p.ExamId == examId && p.ClassId == classId && p.SectionId == sectionId && p.AcademicYear == academicYear)
            .ToListAsync(cancellationToken);

    public async Task<ExamPosition?> GetOneAsync(
        Guid examId, Guid classId, Guid sectionId, Guid studentId, CancellationToken cancellationToken = default)
        => await context.ExamPositions
            .Include(p => p.Student)
            .FirstOrDefaultAsync(p =>
                p.ExamId == examId && p.ClassId == classId && p.SectionId == sectionId && p.StudentId == studentId,
                cancellationToken);

    public async Task UpsertRangeAsync(IEnumerable<ExamPosition> positions, CancellationToken cancellationToken = default)
    {
        foreach (var position in positions)
        {
            var existing = await context.ExamPositions.FirstOrDefaultAsync(p =>
                p.ExamId == position.ExamId &&
                p.ClassId == position.ClassId &&
                p.SectionId == position.SectionId &&
                p.StudentId == position.StudentId, cancellationToken);

            if (existing is null)
            {
                await context.ExamPositions.AddAsync(position, cancellationToken);
                continue;
            }

            existing.AcademicYear = position.AcademicYear;
            existing.TotalMarks = position.TotalMarks;
            existing.FullMarks = position.FullMarks;
            existing.Percentage = position.Percentage;
            existing.Gpa = position.Gpa;
            existing.Result = position.Result;
            existing.Position = position.Position;
            existing.UpdatedAt = DateTime.UtcNow;
            context.ExamPositions.Update(existing);
        }
    }

    public async Task<Dictionary<Guid, decimal>> GetMarkTotalsAsync(
        Guid examId, Guid classId, Guid sectionId, CancellationToken cancellationToken = default)
        => await context.MarkEntries
            .Where(m => m.ExamId == examId && m.ClassId == classId && m.SectionId == sectionId)
            .GroupBy(m => m.StudentId)
            .Select(g => new { StudentId = g.Key, Total = g.Sum(m => m.TotalMark ?? 0) })
            .ToDictionaryAsync(x => x.StudentId, x => x.Total, cancellationToken);

    public async Task<decimal> GetFullMarksAsync(
        Guid examId, Guid classId, Guid sectionId, CancellationToken cancellationToken = default)
    {
        var sum = await context.ExamScheduleSubjects
            .Where(s => s.Schedule.ExamId == examId && s.Schedule.ClassId == classId && s.Schedule.SectionId == sectionId)
            .SumAsync(s => (decimal?)(s.WrittenFullMark ?? 0), cancellationToken);
        return sum ?? 0;
    }
}
