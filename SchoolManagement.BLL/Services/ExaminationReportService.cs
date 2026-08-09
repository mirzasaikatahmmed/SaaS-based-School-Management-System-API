using System.Globalization;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.Reports;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class ExaminationReportService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IStorageService storage,
    IHttpContextAccessor http) : IExaminationReportService
{
    private const decimal DefaultPassPercentage = 33m;

    public async Task<ExamReportStudentListDto> GetStudentsAsync(
        ExamReportStudentFilterDto filter, CancellationToken ct = default)
    {
        await Ready(ct);
        ManageOrTeacher();

        var students = await uow.MarkEntries.GetActiveStudentsByClassSectionAsync(filter.ClassId, filter.SectionId, ct);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            students = students.Where(x =>
                x.FirstName.ToLower().Contains(s) ||
                (x.LastName?.ToLower().Contains(s) ?? false) ||
                x.RegisterNo.ToLower().Contains(s) ||
                (x.Roll?.ToLower().Contains(s) ?? false) ||
                (x.MobileNo?.ToLower().Contains(s) ?? false)).ToList();
        }

        var cls = await uow.ClassControls.GetByIdAsync(filter.ClassId, ct);
        var section = await uow.SectionControls.GetByIdAsync(filter.SectionId, ct);

        return new ExamReportStudentListDto
        {
            ClassId = filter.ClassId,
            SectionId = filter.SectionId,
            AcademicYear = filter.AcademicYear,
            ClassName = cls?.Name,
            SectionName = section?.Name,
            Students = students.Select((x, i) => new ExamReportStudentRowDto
            {
                Sl = i + 1,
                StudentId = x.Id,
                StudentName = StudentName(x),
                Category = x.Category?.Name,
                RegisterNo = x.RegisterNo,
                Roll = x.Roll,
                MobileNo = x.MobileNo
            }).ToList()
        };
    }

    public async Task<ReportCardBatchDto> GenerateReportCardsAsync(
        GenerateExamCardsRequestDto request, CancellationToken ct = default)
    {
        await Ready(ct);
        ManageOrTeacher();

        var examId = request.ExamId ?? request.ExamIds.FirstOrDefault();
        if (examId == Guid.Empty)
            throw new AppException("ExamId is required.", 400);
        if (request.StudentIds.Count == 0)
            throw new AppException("Select at least one student.", 400);

        var card = await BuildCardsForExamAsync(examId, request, ct);
        return new ReportCardBatchDto { Cards = card };
    }

    public async Task<ReportCardBatchDto> GenerateProgressReportsAsync(
        GenerateExamCardsRequestDto request, CancellationToken ct = default)
    {
        await Ready(ct);
        ManageOrTeacher();

        var examIds = request.ExamIds.Count > 0
            ? request.ExamIds.Distinct().ToList()
            : request.ExamId.HasValue ? [request.ExamId.Value] : [];
        if (examIds.Count == 0)
            throw new AppException("At least one exam is required.", 400);
        if (request.StudentIds.Count == 0)
            throw new AppException("Select at least one student.", 400);

        var cards = new List<ReportCardDto>();
        foreach (var examId in examIds)
            cards.AddRange(await BuildCardsForExamAsync(examId, request, ct));

        return new ReportCardBatchDto { Cards = cards };
    }

    public async Task<TabulationSheetDto> GetTabulationSheetAsync(
        Guid examId, Guid classId, Guid sectionId, int academicYear, CancellationToken ct = default)
    {
        await Ready(ct);
        ManageOrTeacher();

        var exam = await uow.Exams.GetByIdAsync(examId, ct)
            ?? throw new NotFoundException("Exam not found.");
        var students = await uow.MarkEntries.GetActiveStudentsByClassSectionAsync(classId, sectionId, ct);
        var scheduleSubjects = await uow.MarkEntries.GetScheduleSubjectsAsync(examId, classId, sectionId, ct);
        var marks = await uow.MarkEntries.GetForExamClassSectionAsync(examId, classId, sectionId, ct);
        var positions = await uow.ExamPositions.GetByFilterAsync(examId, classId, sectionId, academicYear, ct);
        var grades = await ActiveGrades(ct);
        var passThreshold = PassThreshold(grades);

        var subjectCols = scheduleSubjects.Select(s => new TabulationSubjectColumnDto
        {
            SubjectId = s.SubjectId,
            Name = s.Subject?.Name ?? s.SubjectId.ToString(),
            FullMarks = s.WrittenFullMark ?? 0
        }).ToList();

        var markLookup = marks.ToDictionary(m => (m.StudentId, m.SubjectId));
        var positionLookup = positions.ToDictionary(p => p.StudentId);
        var assignment = await uow.ClassSubjectAssignments.GetByClassSectionAsync(classId, sectionId, ct);
        var electiveSubjectIds = assignment?.Items.Where(i => i.IsElective).Select(i => i.SubjectId).ToHashSet()
                                 ?? [];
        var enrollmentsByStudent = (await uow.StudentSubjectEnrollments.GetForClassAsync(
                classId, sectionId, academicYear, null, ct))
            .GroupBy(e => e.StudentId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.SubjectId).ToHashSet());

        var rows = students.Select(stu =>
        {
            enrollmentsByStudent.TryGetValue(stu.Id, out var enrolled);
            enrolled ??= [];

            var subjectMarks = new Dictionary<string, decimal?>();
            decimal total = 0;
            decimal fullForStudent = 0;
            foreach (var col in subjectCols)
            {
                var isElective = electiveSubjectIds.Contains(col.SubjectId);
                if (isElective && !enrolled.Contains(col.SubjectId))
                {
                    subjectMarks[col.SubjectId.ToString()] = null; // not their 4th subject
                    continue;
                }

                fullForStudent += col.FullMarks;
                markLookup.TryGetValue((stu.Id, col.SubjectId), out var m);
                decimal? obt = m is null || m.IsAbsent ? null : m.TotalMark;
                subjectMarks[col.SubjectId.ToString()] = obt;
                total += obt ?? 0;
            }

            positionLookup.TryGetValue(stu.Id, out var pos);
            decimal gpa;
            string result;
            string? positionLabel;
            if (pos is not null)
            {
                gpa = pos.Gpa;
                result = pos.Result;
                positionLabel = pos.Position?.ToString() ?? "Not Generated";
            }
            else
            {
                var pct = fullForStudent > 0 ? Math.Round(total / fullForStudent * 100, 2) : 0;
                var g = ResolveGrade(grades, pct);
                gpa = g?.GradePoint ?? 0;
                result = pct >= passThreshold ? "PASS" : "FAIL";
                positionLabel = "Not Generated";
            }

            return new TabulationRowDto
            {
                Position = positionLabel,
                StudentId = stu.Id,
                StudentName = StudentName(stu),
                RegisterNo = stu.RegisterNo,
                Roll = stu.Roll,
                SubjectMarks = subjectMarks,
                TotalMarks = total,
                Gpa = gpa,
                Result = result
            };
        }).ToList();

        var cls = await uow.ClassControls.GetByIdAsync(classId, ct);
        var section = await uow.SectionControls.GetByIdAsync(sectionId, ct);

        return new TabulationSheetDto
        {
            ExamId = examId,
            ExamName = exam.Name,
            ClassId = classId,
            SectionId = sectionId,
            AcademicYear = academicYear,
            ClassName = cls?.Name,
            SectionName = section?.Name,
            Subjects = subjectCols,
            Rows = rows
        };
    }

    private async Task<IReadOnlyList<ReportCardDto>> BuildCardsForExamAsync(
        Guid examId, GenerateExamCardsRequestDto request, CancellationToken ct)
    {
        var exam = await uow.Exams.GetByIdAsync(examId, ct)
            ?? throw new NotFoundException($"Exam '{examId}' not found.");
        var students = await uow.MarkEntries.GetActiveStudentsByClassSectionAsync(request.ClassId, request.SectionId, ct);
        var selected = students.Where(s => request.StudentIds.Contains(s.Id)).ToList();
        if (selected.Count == 0)
            throw new AppException("No matching students for the selected class/section.", 400);

        var scheduleSubjectsAll = await uow.MarkEntries.GetScheduleSubjectsAsync(examId, request.ClassId, request.SectionId, ct);
        var marks = await uow.MarkEntries.GetForExamClassSectionAsync(examId, request.ClassId, request.SectionId, ct);
        var grades = await ActiveGrades(ct);
        var passThreshold = PassThreshold(grades);
        var positions = (await uow.ExamPositions.GetByFilterAsync(
            examId, request.ClassId, request.SectionId, request.AcademicYear, ct))
            .ToDictionary(p => p.StudentId);

        var markLookup = marks.ToDictionary(m => (m.StudentId, m.SubjectId));
        var subjectPositions = ComputeSubjectPositions(scheduleSubjectsAll, marks);

        var settings = await uow.SchoolSettings.GetAsync(ct);
        var printDate = request.PrintDate?.Date ?? DateTime.UtcNow.Date;
        var printedBy = http.HttpContext?.User.FindFirst("name")?.Value
                        ?? http.HttpContext?.User.Identity?.Name;
        GradeScaleItemDto[]? gradeScale = request.PrintGradeScale
            ? grades.Select(g => new GradeScaleItemDto
            {
                GradeName = g.GradeName,
                GradePoint = g.GradePoint,
                MinPercentage = g.MinPercentage,
                MaxPercentage = g.MaxPercentage,
                Remarks = g.Remarks
            }).ToArray()
            : null;

        var weekends = ParseWeekends(settings?.WeekendDays);
        var yearStart = new DateTime(request.AcademicYear, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var yearEnd = printDate < yearStart ? yearStart : printDate;
        if (yearEnd < yearStart) yearEnd = yearStart;
        var holidays = request.PrintAttendance
            ? (await uow.Events.GetHolidayDatesAsync(yearStart, yearEnd, ct)).ToHashSet()
            : [];

        var subjectMeta = (await uow.Subjects.GetAllAsync(ct)).ToDictionary(s => s.Id);
        var enrollmentsByStudent = (await uow.StudentSubjectEnrollments.GetForClassAsync(
                request.ClassId, request.SectionId, request.AcademicYear, null, ct))
            .GroupBy(e => e.StudentId)
            .ToDictionary(g => g.Key, g => g.First());

        var cards = new List<ReportCardDto>();
        foreach (var stu in selected.OrderBy(s => s.Roll).ThenBy(s => s.RegisterNo))
        {
            var scheduleSubjects = await Helpers.ElectiveSubjectHelper.FilterScheduleSubjectsForStudentAsync(
                uow, request.ClassId, request.SectionId, stu.Id, request.AcademicYear, scheduleSubjectsAll, ct);

            enrollmentsByStudent.TryGetValue(stu.Id, out var enrollment);
            var additionalId = enrollment?.AdditionalSubjectId;

            var allRows = new List<ReportCardSubjectRowDto>();
            var gpInputs = new List<Helpers.BdGpaCalculator.SubjectGp>();
            decimal obtainedSum = 0;
            decimal fullSum = 0;

            foreach (var ss in scheduleSubjects)
            {
                var full = ss.WrittenFullMark ?? 0;
                markLookup.TryGetValue((stu.Id, ss.SubjectId), out var m);
                var obt = m is null || m.IsAbsent ? (decimal?)null : m.TotalMark;

                subjectMeta.TryGetValue(ss.SubjectId, out var meta);
                var isCa = meta?.IsContinuousAssessment ?? false;

                string? gradeName = null;
                decimal? gp = null;
                string? remark = null;
                if (obt.HasValue && full > 0)
                {
                    var pct = Math.Round(obt.Value / full * 100, 2);
                    var g = ResolveGrade(grades, pct);
                    gradeName = g?.GradeName;
                    gp = g?.GradePoint;
                    remark = g?.Remarks;
                }

                // Continuous assessment graded but excluded from GPA / grand total (board style)
                if (!isCa)
                {
                    fullSum += full;
                    obtainedSum += obt ?? 0;
                }

                if (gp.HasValue)
                {
                    gpInputs.Add(new Helpers.BdGpaCalculator.SubjectGp
                    {
                        SubjectId = ss.SubjectId,
                        Name = ss.Subject?.Name ?? ss.SubjectId.ToString(),
                        GradePoint = gp.Value,
                        IsContinuousAssessment = isCa
                    });
                }

                subjectPositions.TryGetValue((ss.SubjectId, stu.Id), out var subPos);

                allRows.Add(new ReportCardSubjectRowDto
                {
                    SubjectId = ss.SubjectId,
                    Subject = ss.Subject?.Name ?? ss.SubjectId.ToString(),
                    ObtainedMarks = obt,
                    FullMarks = full,
                    MarksDisplay = obt.HasValue ? $"{Trim(obt.Value)}/{full}" : $"—/{full}",
                    WrittenMark = m?.WrittenMark,
                    McqMark = m?.McqMark,
                    Grade = gradeName,
                    GradePoint = gp,
                    Remark = remark,
                    SubjectPosition = subPos > 0 ? subPos : null,
                    IsAbsent = m?.IsAbsent ?? false
                });
            }

            ReportCardSubjectRowDto? additionalRow = null;
            if (additionalId.HasValue)
                additionalRow = allRows.FirstOrDefault(r => r.SubjectId == additionalId.Value);

            var continuous = allRows.Where(r =>
                subjectMeta.TryGetValue(r.SubjectId, out var m) && m.IsContinuousAssessment).ToList();
            var mainRows = allRows.Where(r =>
            {
                if (subjectMeta.TryGetValue(r.SubjectId, out var m) && m.IsContinuousAssessment)
                    return false;
                if (additionalId.HasValue && r.SubjectId == additionalId.Value)
                    return false;
                return true;
            }).ToList();

            var bdGpa = Helpers.BdGpaCalculator.Calculate(gpInputs, additionalId);
            var avgPct = fullSum > 0 ? Math.Round(obtainedSum / fullSum * 100, 2) : 0;
            positions.TryGetValue(stu.Id, out var pos);
            var overallGrade = ResolveGrade(grades, pos?.Percentage ?? avgPct);
            // Prefer board-style GPA; fall back to stored position GPA only if no subject GPs
            var gpa = bdGpa.MainSubjectCount > 0 ? bdGpa.Gpa : (pos?.Gpa ?? overallGrade?.GradePoint ?? 0);
            var result = pos?.Result ?? (avgPct >= passThreshold ? "PASS" : "FAIL");

            AttendanceSummaryDto? attendance = null;
            if (request.PrintAttendance)
            {
                var attRecords = await uow.StudentAttendances.GetReportAsync(
                    request.ClassId, request.SectionId, stu.Id, yearStart, yearEnd, ct);
                var attended = attRecords.Count(a =>
                    a.Status.Equals("Present", StringComparison.OrdinalIgnoreCase) ||
                    a.Status.Equals("Late", StringComparison.OrdinalIgnoreCase) ||
                    a.Status.Equals("HalfDay", StringComparison.OrdinalIgnoreCase));
                var working = CountWorkingDays(yearStart, yearEnd, weekends, holidays);
                attendance = new AttendanceSummaryDto
                {
                    WorkingDays = working,
                    DaysAttended = attended,
                    AttendancePercentage = working <= 0 ? 0 : Math.Round((decimal)attended * 100m / working, 2)
                };
            }

            var father = stu.Guardians.FirstOrDefault(g =>
                g.Relation.Contains("Father", StringComparison.OrdinalIgnoreCase))?.Name
                ?? stu.Guardians.FirstOrDefault()?.FatherName
                ?? stu.Guardians.FirstOrDefault(g => g.IsPrimary)?.Name;
            var mother = stu.Guardians.FirstOrDefault(g =>
                g.Relation.Contains("Mother", StringComparison.OrdinalIgnoreCase))?.Name
                ?? stu.Guardians.FirstOrDefault()?.MotherName;

            cards.Add(new ReportCardDto
            {
                StudentId = stu.Id,
                ExamId = examId,
                AcademicYear = request.AcademicYear,
                SchoolName = settings?.SchoolName,
                SchoolAddress = settings?.Address,
                SchoolPhone = settings?.Phone,
                SchoolEmail = settings?.Email,
                SchoolWebsite = settings?.Website,
                LogoUrl = await ResolvePhotoAsync(settings?.ReportCardLogoUrl ?? settings?.PrintingLogoUrl, ct),
                StudentName = StudentName(stu),
                RegisterNo = stu.RegisterNo,
                Roll = stu.Roll,
                ExamName = exam.Name,
                FatherName = father,
                MotherName = mother,
                DateOfBirth = stu.DateOfBirth,
                Gender = stu.Gender,
                ClassName = stu.Class?.Name,
                SectionName = stu.Section?.Name,
                PhotoUrl = await ResolvePhotoAsync(stu.ProfilePictureUrl, ct),
                Subjects = mainRows,
                AdditionalSubject = additionalRow,
                ContinuousAssessment = continuous,
                GrandTotalObtained = obtainedSum,
                GrandTotalFull = fullSum,
                GrandTotalDisplay = $"{Trim(obtainedSum)}/{Trim(fullSum)}",
                GrandTotalInWords = NumberToWords((int)Math.Round(obtainedSum)),
                AveragePercentage = pos?.Percentage ?? avgPct,
                Gpa = gpa,
                GpaWithoutAdditional = bdGpa.GpaWithoutAdditional,
                AdditionalGpAbove2 = bdGpa.AdditionalGpAbove2,
                OverallGrade = overallGrade?.GradeName,
                Result = result,
                Position = pos?.Position,
                Attendance = attendance,
                GradeScale = gradeScale,
                PrintDate = printDate,
                PrintedBy = printedBy
            });
        }

        return cards;
    }

    private static Dictionary<(Guid SubjectId, Guid StudentId), int> ComputeSubjectPositions(
        IReadOnlyList<ExamScheduleSubject> subjects, IReadOnlyList<MarkEntry> marks)
    {
        var result = new Dictionary<(Guid, Guid), int>();
        foreach (var ss in subjects)
        {
            var ranked = marks
                .Where(m => m.SubjectId == ss.SubjectId && !m.IsAbsent && m.TotalMark.HasValue)
                .OrderByDescending(m => m.TotalMark)
                .ToList();
            var rank = 0;
            decimal last = decimal.MinValue;
            for (var i = 0; i < ranked.Count; i++)
            {
                var m = ranked[i];
                if (m.TotalMark != last)
                {
                    rank = i + 1;
                    last = m.TotalMark!.Value;
                }
                result[(ss.SubjectId, m.StudentId)] = rank;
            }
        }
        return result;
    }

    private async Task<List<GradeRange>> ActiveGrades(CancellationToken ct)
        => (await uow.GradeRanges.GetAllAsync(ct)).Where(g => g.IsActive).OrderBy(g => g.SortOrder).ToList();

    private static decimal PassThreshold(IReadOnlyList<GradeRange> grades)
        => grades.Where(g => g.GradePoint > 0).Select(g => (decimal?)g.MinPercentage).Min() ?? DefaultPassPercentage;

    private static GradeRange? ResolveGrade(IReadOnlyList<GradeRange> grades, decimal percentage)
        => grades.FirstOrDefault(g => percentage >= g.MinPercentage && percentage <= g.MaxPercentage);

    private static HashSet<DayOfWeek> ParseWeekends(string? raw)
    {
        raw = string.IsNullOrWhiteSpace(raw) ? "5,6" : raw;
        var set = new HashSet<DayOfWeek>();
        foreach (var part in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            if (int.TryParse(part, out var n) && n is >= 0 and <= 6)
                set.Add((DayOfWeek)n);
        if (set.Count == 0) { set.Add(DayOfWeek.Friday); set.Add(DayOfWeek.Saturday); }
        return set;
    }

    private static int CountWorkingDays(DateTime from, DateTime to, HashSet<DayOfWeek> weekends, HashSet<DateTime> holidays)
    {
        var count = 0;
        for (var d = from.Date; d <= to.Date; d = d.AddDays(1))
        {
            if (weekends.Contains(d.DayOfWeek)) continue;
            if (holidays.Contains(d)) continue;
            count++;
        }
        return count;
    }

    private async Task<string?> ResolvePhotoAsync(string? objectKey, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(objectKey) || string.IsNullOrEmpty(tenant.TenantSlug))
            return objectKey;
        try { return await storage.GetPresignedUrlAsync(tenant.TenantSlug!, objectKey, ct); }
        catch { return objectKey; }
    }

    private static string StudentName(Student s)
        => string.IsNullOrWhiteSpace(s.LastName) ? s.FirstName.Trim() : $"{s.FirstName.Trim()} {s.LastName.Trim()}";

    private static string Trim(decimal v)
        => v == Math.Floor(v) ? ((int)v).ToString(CultureInfo.InvariantCulture) : v.ToString("0.##", CultureInfo.InvariantCulture);

    private static string NumberToWords(int number)
    {
        if (number == 0) return "Zero";
        if (number < 0) return "Minus " + NumberToWords(Math.Abs(number));

        var words = new StringBuilder();
        if (number / 1000000 > 0)
        {
            words.Append(NumberToWords(number / 1000000)).Append(" Million ");
            number %= 1000000;
        }
        if (number / 1000 > 0)
        {
            words.Append(NumberToWords(number / 1000)).Append(" Thousand ");
            number %= 1000;
        }
        if (number / 100 > 0)
        {
            words.Append(NumberToWords(number / 100)).Append(" Hundred ");
            number %= 100;
        }
        if (number > 0)
        {
            if (words.Length > 0) words.Append("and ");
            var unitsMap = new[]
            {
                "Zero","One","Two","Three","Four","Five","Six","Seven","Eight","Nine","Ten",
                "Eleven","Twelve","Thirteen","Fourteen","Fifteen","Sixteen","Seventeen","Eighteen","Nineteen"
            };
            var tensMap = new[]
            {
                "Zero","Ten","Twenty","Thirty","Forty","Fifty","Sixty","Seventy","Eighty","Ninety"
            };
            if (number < 20) words.Append(unitsMap[number]);
            else
            {
                words.Append(tensMap[number / 10]);
                if (number % 10 > 0) words.Append('-').Append(unitsMap[number % 10]);
            }
        }
        return words.ToString().Trim();
    }

    private async Task Ready(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(tenant.SchemaName))
            throw new AppException("X-Tenant-ID header is required.", 400);
        await provisioner.EnsureSettingsModuleAsync(tenant.SchemaName!, ct);
    }

    private HashSet<string> Roles() =>
        http.HttpContext?.User.FindAll("role").Concat(http.HttpContext.User.FindAll(ClaimTypes.Role))
            .Select(x => x.Value).ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];

    private void ManageOrTeacher()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin)
            && !r.Contains(AppConstants.Roles.Teacher))
            throw new ForbiddenException("Only Super Admin, School Admin, or Teacher can access examination reports.");
    }
}
