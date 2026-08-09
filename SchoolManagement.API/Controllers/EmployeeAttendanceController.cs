using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.Attendance;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/attendance/employee")]
[Authorize]
public class EmployeeAttendanceController(IEmployeeAttendanceService service) : ControllerBase
{
    private const string ManageRoles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}";

    [HttpGet]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> GetForDate([FromQuery] string? role, [FromQuery] DateTime date, CancellationToken ct = default)
        => Ok(ApiResponse<EmployeeAttendanceForDateResponseDto>.Ok(await service.GetForDateAsync(role, date, ct), "Employee attendance retrieved"));

    [HttpPatch("save")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Save(SaveEmployeeAttendanceDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<EmployeeAttendanceForDateResponseDto>.Ok(await service.SaveAsync(dto, ct), "Employee attendance saved"));

    [HttpGet("report")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> GetReport([FromQuery] EmployeeAttendanceReportFilterDto filter, CancellationToken ct = default)
        => Ok(ApiResponse<EmployeeAttendanceReportResponseDto>.Ok(await service.GetReportAsync(filter, ct), "Employee attendance report retrieved"));

    [HttpGet("report/my")]
    public async Task<IActionResult> GetMyReport([FromQuery] EmployeeAttendanceReportFilterDto filter, CancellationToken ct = default)
        => Ok(ApiResponse<EmployeeAttendanceReportResponseDto>.Ok(await service.GetMyReportAsync(filter, ct), "My attendance report retrieved"));
}
