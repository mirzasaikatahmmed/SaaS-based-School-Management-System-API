using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.Marks;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class ExamPositionService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http) : IExamPositionService
{
    private const string PassResult = "PASS";
    private const string FailResult = "FAIL";
    private const decimal DefaultPassPercentage = 33m;

    public async Task<IReadOnlyList<ExamPositionItemDto>> GetAsync(ExamPositionFilterDto filter, CancellationToken ct = default)
    {
        await Ready(ct);
        ManageOrTeacher();

        var positions = await uow.ExamPositions.GetByFilterAsync(filter.ExamId, filter.ClassId, filter.SectionId, filter.AcademicYear, ct);
        return positions
            .OrderBy(p => p.Position ?? int.MaxValue)
            .ThenByDescending(p => p.TotalMarks)
            .Select((p, i) => MapItem(p, i + 1))
            .ToList();
    }

    public async Task<IReadOnlyList<ExamPositionItemDto>> GenerateAsync(ExamPositionFilterDto filter, CancellationToken ct = default)
    {
        await Ready(ct);
        ManageOrTeacher();

        if (await uow.Exams.GetByIdAsync(filter.ExamId, ct) is null)
            throw new NotFoundException($"Exam '{filter.ExamId}' not found.");

        var students = await uow.MarkEntries.GetActiveStudentsByClassSectionAsync(filter.ClassId, filter.SectionId, ct);
        if (students.Count == 0)
            throw new AppException("No active students found for the selected class and section.", 400);

        var totals = await uow.ExamPositions.GetMarkTotalsAsync(filter.ExamId, filter.ClassId, filter.SectionId, ct);
        var fullMarks = await uow.ExamPositions.GetFullMarksAsync(filter.ExamId, filter.ClassId, filter.SectionId, ct);
        var grades = (await uow.GradeRanges.GetAllAsync(ct)).Where(g => g.IsActive).OrderBy(g => g.SortOrder).ToList();
        var passThreshold = grades.Where(g => g.GradePoint > 0).Select(g => (decimal?)g.MinPercentage).Min() ?? DefaultPassPercentage;

        var scheduleSubjectsAll = await uow.MarkEntries.GetScheduleSubjectsAsync(filter.ExamId, filter.ClassId, filter.SectionId, ct);
        var marks = await uow.MarkEntries.GetForExamClassSectionAsync(filter.ExamId, filter.ClassId, filter.SectionId, ct);
        var markLookup = marks.ToDictionary(m => (m.StudentId, m.SubjectId));
        var subjectMeta = (await uow.Subjects.GetAllAsync(ct)).ToDictionary(s => s.Id);
        var enrollmentsByStudent = (await uow.StudentSubjectEnrollments.GetForClassAsync(
                filter.ClassId, filter.SectionId, filter.AcademicYear, null, ct))
            .GroupBy(e => e.StudentId)
            .ToDictionary(g => g.Key, g => g.First());

        var computed = new List<(Student Student, decimal Total, decimal Percentage, decimal Gpa, string Result)>();
        foreach (var student in students)
        {
            totals.TryGetValue(student.Id, out var total);
            var percentage = fullMarks > 0 ? Math.Round(total / fullMarks * 100, 2) : 0;

            var scheduleSubjects = await Helpers.ElectiveSubjectHelper.FilterScheduleSubjectsForStudentAsync(
                uow, filter.ClassId, filter.SectionId, student.Id, filter.AcademicYear, scheduleSubjectsAll, ct);
            enrollmentsByStudent.TryGetValue(student.Id, out var enrollment);

            var gpInputs = new List<Helpers.BdGpaCalculator.SubjectGp>();
            foreach (var ss in scheduleSubjects)
            {
                markLookup.TryGetValue((student.Id, ss.SubjectId), out var m);
                var obt = m is null || m.IsAbsent ? (decimal?)null : m.TotalMark;
                var full = ss.WrittenFullMark ?? 0;
                if (!obt.HasValue || full <= 0) continue;

                subjectMeta.TryGetValue(ss.SubjectId, out var meta);
                var pct = Math.Round(obt.Value / full * 100, 2);
                var g = grades.FirstOrDefault(x => pct >= x.MinPercentage && pct <= x.MaxPercentage);
                if (g is null) continue;

                gpInputs.Add(new Helpers.BdGpaCalculator.SubjectGp
                {
                    SubjectId = ss.SubjectId,
                    Name = ss.Subject?.Name ?? ss.SubjectId.ToString(),
                    GradePoint = g.GradePoint,
                    IsContinuousAssessment = meta?.IsContinuousAssessment ?? false
                });
            }

            var bd = Helpers.BdGpaCalculator.Calculate(gpInputs, enrollment?.AdditionalSubjectId);
            var gpa = bd.MainSubjectCount > 0
                ? bd.Gpa
                : grades.FirstOrDefault(x => percentage >= x.MinPercentage && percentage <= x.MaxPercentage)?.GradePoint ?? 0;
            var result = percentage >= passThreshold ? PassResult : FailResult;
            computed.Add((student, total, percentage, gpa, result));
        }

        var ranked = computed.OrderByDescending(x => x.Total).ToList();
        var positions = new List<ExamPosition>();
        var rank = 0;
        var lastTotal = decimal.MinValue;
        for (var i = 0; i < ranked.Count; i++)
        {
            var (student, total, percentage, gpa, result) = ranked[i];
            if (total != lastTotal)
            {
                rank = i + 1;
                lastTotal = total;
            }

            positions.Add(new ExamPosition
            {
                Id = Guid.NewGuid(),
                ExamId = filter.ExamId,
                ClassId = filter.ClassId,
                SectionId = filter.SectionId,
                StudentId = student.Id,
                AcademicYear = filter.AcademicYear,
                TotalMarks = total,
                FullMarks = fullMarks,
                Percentage = percentage,
                Gpa = gpa,
                Result = result,
                Position = rank,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await uow.ExamPositions.UpsertRangeAsync(positions, ct);
        await uow.SaveTenantChangesAsync(ct);

        return await GetAsync(filter, ct);
    }

    public async Task<IReadOnlyList<ExamPositionItemDto>> SaveAsync(SaveExamPositionDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        ManageOrTeacher();

        var positions = new List<ExamPosition>();
        foreach (var item in dto.Items)
        {
            var existing = await uow.ExamPositions.GetOneAsync(dto.ExamId, dto.ClassId, dto.SectionId, item.StudentId, ct);
            var entity = existing ?? new ExamPosition
            {
                Id = Guid.NewGuid(),
                ExamId = dto.ExamId,
                ClassId = dto.ClassId,
                SectionId = dto.SectionId,
                StudentId = item.StudentId,
                AcademicYear = dto.AcademicYear,
                Result = FailResult,
                CreatedAt = DateTime.UtcNow
            };

            entity.Position = item.Position;
            entity.PrincipalComments = item.PrincipalComments?.Trim();
            entity.TeacherComments = item.TeacherComments?.Trim();
            entity.UpdatedAt = DateTime.UtcNow;
            positions.Add(entity);
        }

        await uow.ExamPositions.UpsertRangeAsync(positions, ct);
        await uow.SaveTenantChangesAsync(ct);

        return await GetAsync(new ExamPositionFilterDto
        {
            ExamId = dto.ExamId,
            ClassId = dto.ClassId,
            SectionId = dto.SectionId,
            AcademicYear = dto.AcademicYear
        }, ct);
    }

    private static ExamPositionItemDto MapItem(ExamPosition p, int sl) => new()
    {
        Id = p.Id,
        Sl = sl,
        StudentId = p.StudentId,
        StudentName = StudentName(p.Student),
        RegisterNo = p.Student.RegisterNo,
        Roll = p.Student.Roll,
        Category = p.Student.Category?.Name,
        TotalMarks = p.TotalMarks,
        FullMarks = p.FullMarks,
        TotalMarksDisplay = $"{TrimDecimal(p.TotalMarks)}/{TrimDecimal(p.FullMarks)}",
        Percentage = p.Percentage,
        Gpa = p.Gpa,
        Result = p.Result,
        Position = p.Position,
        PrincipalComments = p.PrincipalComments,
        TeacherComments = p.TeacherComments
    };

    private static string TrimDecimal(decimal value) => value % 1 == 0 ? ((int)value).ToString() : value.ToString("0.##");

    private static string StudentName(Student s)
        => string.IsNullOrWhiteSpace(s.LastName) ? s.FirstName.Trim() : $"{s.FirstName.Trim()} {s.LastName.Trim()}";

    private async Task Ready(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(tenant.SchemaName))
            throw new AppException("X-Tenant-ID header is required.", 400);
        await provisioner.EnsureGradesAttendanceLibraryEventsModuleAsync(tenant.SchemaName!, ct);
    }

    private HashSet<string> Roles()
    {
        var p = http.HttpContext?.User;
        if (p is null) return [];
        return p.FindAll("role").Concat(p.FindAll(ClaimTypes.Role)).Select(x => x.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private void ManageOrTeacher()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin) &&
            !r.Contains(AppConstants.Roles.Teacher))
            throw new ForbiddenException("Only Super Admin, School Admin, or Teacher can manage exam positions.");
    }
}
