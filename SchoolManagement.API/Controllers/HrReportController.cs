using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.API.Filters;
using SchoolManagement.BLL.DTOs.Reports;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

/// <summary>Reports → Human Resource (Leave Reports, Payroll Summary).</summary>
[ApiController]
[Route("api/reports/hr")]
[Authorize]
public class HrReportController(IHrReportService service) : ControllerBase
{
    [HttpGet("leave")]
    [AuthorizePermission("Reports.Leave", AppConstants.PermissionActions.View)]
    public async Task<IActionResult> Leave(
        [FromQuery] string? role,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] string? status,
        [FromQuery] string? search,
        [FromQuery] string? export,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100,
        CancellationToken ct = default)
    {
        var result = await service.GetLeaveReportAsync(new LeaveReportFilterDto
        {
            Role = role,
            FromDate = fromDate,
            ToDate = toDate,
            Status = status,
            Search = search,
            Export = export,
            Page = page,
            PageSize = pageSize
        }, ct);

        if (string.Equals(export, "csv", StringComparison.OrdinalIgnoreCase))
            return File(Encoding.UTF8.GetBytes(ToLeaveCsv(result.Rows)), "text/csv", "leave-report.csv");

        return Ok(ApiResponse<LeaveReportDto>.Ok(result, "Leave report retrieved"));
    }

    [HttpGet("payroll-summary")]
    [AuthorizePermission("Reports.PayrollSummary", AppConstants.PermissionActions.View)]
    public async Task<IActionResult> PayrollSummary(
        [FromQuery] string month,
        [FromQuery] string? role,
        [FromQuery] string? search,
        [FromQuery] string? export,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100,
        CancellationToken ct = default)
    {
        var result = await service.GetPayrollSummaryAsync(new PayrollSummaryFilterDto
        {
            Month = month,
            Role = role,
            Search = search,
            Export = export,
            Page = page,
            PageSize = pageSize
        }, ct);

        if (string.Equals(export, "csv", StringComparison.OrdinalIgnoreCase))
            return File(Encoding.UTF8.GetBytes(ToPayrollCsv(result.Rows)), "text/csv", "payroll-summary.csv");

        return Ok(ApiResponse<PayrollSummaryReportDto>.Ok(result, "Payroll summary retrieved"));
    }

    private static string ToLeaveCsv(IEnumerable<LeaveReportRowDto> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Sl,Role,Applicant,LeaveCategory,DateOfStart,DateOfEnd,Days,ApplyDate,Status");
        foreach (var r in rows)
        {
            sb.AppendLine(string.Join(',',
                r.Sl, Csv(r.Role), Csv(r.Applicant), Csv(r.LeaveCategory),
                r.DateOfStart.ToString("yyyy-MM-dd"), r.DateOfEnd.ToString("yyyy-MM-dd"),
                r.Days, r.ApplyDate.ToString("yyyy-MM-dd"), Csv(r.Status)));
        }
        return sb.ToString();
    }

    private static string ToPayrollCsv(IEnumerable<PayrollSummaryRowDto> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Sl,Name,Designation,Salary,Allowance,Deduction,NetSalary,PayVia,Status");
        foreach (var r in rows)
        {
            sb.AppendLine(string.Join(',',
                r.Sl, Csv(r.Name), Csv(r.Designation),
                r.Salary.ToString("0.00"), r.Allowance.ToString("0.00"),
                r.Deduction.ToString("0.00"), r.NetSalary.ToString("0.00"),
                Csv(r.PayVia), Csv(r.Status)));
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
