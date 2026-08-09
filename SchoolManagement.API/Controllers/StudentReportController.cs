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
[Route("api/reports/students")]
[Authorize]
public class StudentReportController(IStudentReportService service) : ControllerBase
{
    [HttpGet("login-credentials")]
    [AuthorizePermission("Reports.StudentLoginCredential", AppConstants.PermissionActions.View)]
    public async Task<IActionResult> LoginCredentials(
        [FromQuery] Guid? classId,
        [FromQuery] Guid? sectionId,
        [FromQuery] string? search,
        [FromQuery] string? export,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await service.GetLoginCredentialsAsync(new StudentReportFilterDto
        {
            ClassId = classId,
            SectionId = sectionId,
            Search = search,
            Export = export,
            Page = page,
            PageSize = pageSize
        }, ct);

        if (string.Equals(export, "csv", StringComparison.OrdinalIgnoreCase))
            return File(Encoding.UTF8.GetBytes(ToLoginCsv(result.Data)), "text/csv", "login-credentials.csv");

        return Ok(ApiResponse<LoginCredentialReportDto>.Ok(result, "Login credential report retrieved"));
    }

    [HttpPost("login-credentials/{studentId:guid}/reset-password")]
    [AuthorizePermission("Reports.StudentLoginCredential", AppConstants.PermissionActions.Edit)]
    public async Task<IActionResult> ResetPassword(Guid studentId, ResetStudentPasswordDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<ResetStudentPasswordResultDto>.Ok(
            await service.ResetPasswordAsync(studentId, dto, ct),
            "Password reset. Save the new credentials — they are shown once."));

    [HttpGet("admission")]
    [AuthorizePermission("Reports.StudentAdmission", AppConstants.PermissionActions.View)]
    public async Task<IActionResult> Admission(
        [FromQuery] Guid? classId,
        [FromQuery] Guid? sectionId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? search,
        [FromQuery] string? export,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await service.GetAdmissionReportAsync(new StudentReportFilterDto
        {
            ClassId = classId,
            SectionId = sectionId,
            FromDate = fromDate,
            ToDate = toDate,
            Search = search,
            Export = export,
            Page = page,
            PageSize = pageSize
        }, ct);

        if (string.Equals(export, "csv", StringComparison.OrdinalIgnoreCase))
            return File(Encoding.UTF8.GetBytes(ToAdmissionCsv(result.Data)), "text/csv", "admission-report.csv");

        return Ok(ApiResponse<AdmissionReportDto>.Ok(result, "Admission report retrieved"));
    }

    [HttpGet("class-section")]
    [AuthorizePermission("Reports.StudentClassSection", AppConstants.PermissionActions.View)]
    public async Task<IActionResult> ClassSection(CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<ClassSectionReportRowDto>>.Ok(
            await service.GetClassSectionReportAsync(ct), "Class & section report retrieved"));

    [HttpGet("siblings")]
    [AuthorizePermission("Reports.StudentSibling", AppConstants.PermissionActions.View)]
    public async Task<IActionResult> Siblings(
        [FromQuery] Guid? classId,
        [FromQuery] Guid? sectionId,
        [FromQuery] string? search,
        CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<SiblingReportRowDto>>.Ok(
            await service.GetSiblingReportAsync(new StudentReportFilterDto
            {
                ClassId = classId,
                SectionId = sectionId,
                Search = search
            }, ct), "Sibling report retrieved"));

    private static string ToLoginCsv(IEnumerable<LoginCredentialRowDto> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Sl,Name,Class,Section,RegisterNo,Roll,GuardianName,StudentUsername,StudentPassword,ParentUsername,ParentPassword");
        foreach (var r in rows)
        {
            sb.AppendLine(string.Join(',',
                r.Sl, Csv(r.Name), Csv(r.ClassName), Csv(r.SectionName), Csv(r.RegisterNo), Csv(r.Roll),
                Csv(r.GuardianName), Csv(r.StudentUsername), Csv(r.StudentPassword),
                Csv(r.ParentUsername), Csv(r.ParentPassword)));
        }
        return sb.ToString();
    }

    private static string ToAdmissionCsv(IEnumerable<AdmissionReportRowDto> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Sl,Name,Gender,RegisterNo,Roll,Class,Section,GuardianName,AdmissionDate");
        foreach (var r in rows)
        {
            sb.AppendLine(string.Join(',',
                r.Sl, Csv(r.Name), Csv(r.Gender), Csv(r.RegisterNo), Csv(r.Roll),
                Csv(r.ClassName), Csv(r.SectionName), Csv(r.GuardianName), r.AdmissionDate.ToString("yyyy-MM-dd")));
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
