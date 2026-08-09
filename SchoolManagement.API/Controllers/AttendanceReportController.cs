using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.API.Filters;
using SchoolManagement.BLL.DTOs.Reports;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/reports/attendance")]
[Authorize]
public class AttendanceReportController(IAttendanceReportService service) : ControllerBase
{
    [HttpGet("students/monthly")]
    [AuthorizePermission("Reports.AttendanceStudent", AppConstants.PermissionActions.View)]
    public async Task<IActionResult> StudentMonthly(
        [FromQuery] Guid classId,
        [FromQuery] Guid sectionId,
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken ct = default)
        => Ok(ApiResponse<MonthlyAttendanceGridDto>.Ok(
            await service.GetStudentMonthlyAsync(classId, sectionId, year, month, ct),
            "Student monthly attendance report retrieved"));

    [HttpGet("students/daily")]
    [AuthorizePermission("Reports.AttendanceStudentDaily", AppConstants.PermissionActions.View)]
    public async Task<IActionResult> StudentDaily([FromQuery] DateTime date, CancellationToken ct = default)
        => Ok(ApiResponse<StudentDailyClassReportDto>.Ok(
            await service.GetStudentDailyAsync(date, ct),
            "Student daily class report retrieved"));

    [HttpGet("students/overview")]
    [AuthorizePermission("Reports.AttendanceStudentOverview", AppConstants.PermissionActions.View)]
    public async Task<IActionResult> StudentOverview(
        [FromQuery] Guid classId,
        [FromQuery] Guid sectionId,
        [FromQuery] string attendanceType,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] string? export,
        CancellationToken ct = default)
    {
        var result = await service.GetStudentOverviewAsync(classId, sectionId, attendanceType, fromDate, toDate, ct);
        if (string.Equals(export, "csv", StringComparison.OrdinalIgnoreCase))
            return File(Encoding.UTF8.GetBytes(ToOverviewCsv(result.Rows)), "text/csv", "student-overview-attendance.csv");
        return Ok(ApiResponse<StudentOverviewReportDto>.Ok(result, "Student overview attendance report retrieved"));
    }

    [HttpGet("subject-wise")]
    [AuthorizePermission("Reports.AttendanceSubjectWise", AppConstants.PermissionActions.View)]
    public async Task<IActionResult> SubjectWise(
        [FromQuery] Guid classId,
        [FromQuery] Guid sectionId,
        [FromQuery] Guid subjectId,
        [FromQuery] DateTime date,
        CancellationToken ct = default)
        => Ok(ApiResponse<SubjectWiseByDateReportDto>.Ok(
            await service.GetSubjectWiseAsync(classId, sectionId, subjectId, date, ct),
            "Subject-wise attendance report retrieved"));

    [HttpGet("subject-wise/by-day")]
    [AuthorizePermission("Reports.AttendanceSubjectWiseByDay", AppConstants.PermissionActions.View)]
    public async Task<IActionResult> SubjectWiseByDay(
        [FromQuery] Guid classId,
        [FromQuery] Guid sectionId,
        [FromQuery] DateTime date,
        CancellationToken ct = default)
        => Ok(ApiResponse<SubjectWiseDayReportDto>.Ok(
            await service.GetSubjectWiseByDayAsync(classId, sectionId, date, ct),
            "Subject-wise by day report retrieved"));

    [HttpGet("subject-wise/by-month")]
    [AuthorizePermission("Reports.AttendanceSubjectWiseByMonth", AppConstants.PermissionActions.View)]
    public async Task<IActionResult> SubjectWiseByMonth(
        [FromQuery] Guid classId,
        [FromQuery] Guid sectionId,
        [FromQuery] Guid subjectId,
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken ct = default)
        => Ok(ApiResponse<MonthlyAttendanceGridDto>.Ok(
            await service.GetSubjectWiseByMonthAsync(classId, sectionId, subjectId, year, month, ct),
            "Subject-wise by month report retrieved"));

    [HttpGet("employees/monthly")]
    [AuthorizePermission("Reports.AttendanceEmployee", AppConstants.PermissionActions.View)]
    public async Task<IActionResult> EmployeeMonthly(
        [FromQuery] string? role,
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken ct = default)
        => Ok(ApiResponse<MonthlyAttendanceGridDto>.Ok(
            await service.GetEmployeeMonthlyAsync(role, year, month, ct),
            "Employee monthly attendance report retrieved"));

    [HttpGet("exams")]
    [AuthorizePermission("Reports.AttendanceExam", AppConstants.PermissionActions.View)]
    public async Task<IActionResult> Exam(
        [FromQuery] Guid examId,
        [FromQuery] Guid classId,
        [FromQuery] Guid sectionId,
        [FromQuery] Guid subjectId,
        [FromQuery] string? export,
        CancellationToken ct = default)
    {
        var result = await service.GetExamReportAsync(examId, classId, sectionId, subjectId, ct);
        if (string.Equals(export, "csv", StringComparison.OrdinalIgnoreCase))
            return File(Encoding.UTF8.GetBytes(ToExamCsv(result.Rows)), "text/csv", "exam-attendance.csv");
        return Ok(ApiResponse<ExamAttendanceReportDto>.Ok(result, "Exam attendance report retrieved"));
    }

    /// <summary>
    /// Fingerprint / biometric punch logs — one row per punch with exact timestamps.
    /// </summary>
    [HttpGet("fingerprint")]
    [AuthorizePermission("Reports.AttendanceFingerprint", AppConstants.PermissionActions.View)]
    public async Task<IActionResult> Fingerprint(
        [FromQuery] string? role,
        [FromQuery] Guid? classId,
        [FromQuery] Guid? sectionId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] Guid? deviceId,
        [FromQuery] string? kind,
        [FromQuery] Guid? studentId,
        [FromQuery] Guid? employeeId,
        [FromQuery] string? devicePin,
        [FromQuery] string? search,
        [FromQuery] string? export,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100,
        CancellationToken ct = default)
    {
        var result = await service.GetFingerprintLogsAsync(new FingerprintLogFilterDto
        {
            Role = role,
            ClassId = classId,
            SectionId = sectionId,
            From = from,
            To = to,
            DeviceId = deviceId,
            Kind = kind,
            StudentId = studentId,
            EmployeeId = employeeId,
            DevicePin = devicePin,
            Search = search,
            Export = export,
            Page = page,
            PageSize = pageSize
        }, ct);

        if (string.Equals(export, "csv", StringComparison.OrdinalIgnoreCase))
            return File(Encoding.UTF8.GetBytes(ToFingerprintCsv(result.Rows)), "text/csv", "fingerprint-attendance-logs.csv");

        return Ok(ApiResponse<FingerprintLogReportDto>.Ok(result, "Fingerprint attendance logs retrieved"));
    }

    private static string ToOverviewCsv(IEnumerable<StudentOverviewReportRowDto> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine("StudentName,RegisterNo,AdmissionDate,Category,Class,Gender,MobileNo,Count");
        foreach (var r in rows)
        {
            sb.AppendLine(string.Join(',',
                Csv(r.StudentName), Csv(r.RegisterNo), r.AdmissionDate.ToString("yyyy-MM-dd"),
                Csv(r.Category), Csv(r.ClassName), Csv(r.Gender), Csv(r.MobileNo), r.Count));
        }
        return sb.ToString();
    }

    private static string ToExamCsv(IEnumerable<ExamAttendanceReportRowDto> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Sl,Name,RegisterNo,Roll,Subject,Remarks,Status");
        foreach (var r in rows)
        {
            sb.AppendLine(string.Join(',',
                r.Sl, Csv(r.Name), Csv(r.RegisterNo), Csv(r.Roll), Csv(r.Subject), Csv(r.Remarks), Csv(r.Status)));
        }
        return sb.ToString();
    }

    private static string ToFingerprintCsv(IEnumerable<FingerprintLogRowDto> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Sl,Name,Roll,AdmissionNo,RegisterId,PunchTimeIso,PunchDate,PunchClock,TerminalId,DeviceName,Role,Class,Section,PunchKind,StatusApplied,ReceivedAtIso");
        foreach (var r in rows)
        {
            sb.AppendLine(string.Join(',',
                r.Sl, Csv(r.Name), Csv(r.Roll), Csv(r.AdmissionNo), Csv(r.RegisterId),
                Csv(r.PunchTimeIso), Csv(r.PunchDate), Csv(r.PunchClock), Csv(r.TerminalId), Csv(r.DeviceName),
                Csv(r.Role), Csv(r.ClassName), Csv(r.SectionName), Csv(r.PunchKind), Csv(r.StatusApplied),
                Csv(r.ReceivedAtIso)));
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
