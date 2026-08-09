using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.API.Filters;
using SchoolManagement.BLL.DTOs.Reports;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

/// <summary>Reports → Examination (Report Card, Tabulation Sheet, Progress Reports).</summary>
[ApiController]
[Route("api/reports/examination")]
[Authorize]
public class ExaminationReportController(IExaminationReportService service) : ControllerBase
{
    [HttpGet("students")]
    [AuthorizePermission("Reports.ExamReportCard", AppConstants.PermissionActions.View)]
    public async Task<IActionResult> Students(
        [FromQuery] Guid classId,
        [FromQuery] Guid sectionId,
        [FromQuery] int academicYear,
        [FromQuery] Guid? examId,
        [FromQuery] string? search,
        CancellationToken ct = default)
        => Ok(ApiResponse<ExamReportStudentListDto>.Ok(
            await service.GetStudentsAsync(new ExamReportStudentFilterDto
            {
                ClassId = classId,
                SectionId = sectionId,
                AcademicYear = academicYear,
                ExamId = examId,
                Search = search
            }, ct),
            "Student list retrieved"));

    [HttpPost("report-card")]
    [AuthorizePermission("Reports.ExamReportCard", AppConstants.PermissionActions.View)]
    public async Task<IActionResult> ReportCard([FromBody] GenerateExamCardsRequestDto request, CancellationToken ct = default)
        => Ok(ApiResponse<ReportCardBatchDto>.Ok(
            await service.GenerateReportCardsAsync(request, ct),
            "Report cards generated"));

    [HttpPost("progress")]
    [AuthorizePermission("Reports.ExamProgress", AppConstants.PermissionActions.View)]
    public async Task<IActionResult> Progress([FromBody] GenerateExamCardsRequestDto request, CancellationToken ct = default)
        => Ok(ApiResponse<ReportCardBatchDto>.Ok(
            await service.GenerateProgressReportsAsync(request, ct),
            "Progress reports generated"));

    [HttpGet("tabulation")]
    [AuthorizePermission("Reports.ExamTabulation", AppConstants.PermissionActions.View)]
    public async Task<IActionResult> Tabulation(
        [FromQuery] Guid examId,
        [FromQuery] Guid classId,
        [FromQuery] Guid sectionId,
        [FromQuery] int academicYear,
        [FromQuery] string? export,
        CancellationToken ct = default)
    {
        var result = await service.GetTabulationSheetAsync(examId, classId, sectionId, academicYear, ct);
        if (string.Equals(export, "csv", StringComparison.OrdinalIgnoreCase))
            return File(Encoding.UTF8.GetBytes(ToTabulationCsv(result)), "text/csv", "tabulation-sheet.csv");
        return Ok(ApiResponse<TabulationSheetDto>.Ok(result, "Tabulation sheet retrieved"));
    }

    private static string ToTabulationCsv(TabulationSheetDto sheet)
    {
        var sb = new StringBuilder();
        var headers = new List<string> { "Position", "Student", "RegisterNo", "Roll" };
        headers.AddRange(sheet.Subjects.Select(s => $"{s.Name} ({s.FullMarks})"));
        headers.AddRange(["TotalMarks", "GPA", "Result"]);
        sb.AppendLine(string.Join(',', headers.Select(Csv)));

        foreach (var r in sheet.Rows)
        {
            var cells = new List<string>
            {
                Csv(r.Position), Csv(r.StudentName), Csv(r.RegisterNo), Csv(r.Roll)
            };
            foreach (var sub in sheet.Subjects)
            {
                r.SubjectMarks.TryGetValue(sub.SubjectId.ToString(), out var mark);
                cells.Add(mark?.ToString("0.##") ?? "");
            }
            cells.Add(r.TotalMarks.ToString("0.##"));
            cells.Add(r.Gpa.ToString("0.##"));
            cells.Add(Csv(r.Result));
            sb.AppendLine(string.Join(',', cells));
        }
        return sb.ToString();
    }

    private static string Csv(string? value)
    {
        value ??= string.Empty;
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
