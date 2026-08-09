using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.Attendance;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/attendance/student")]
[Authorize]
public class StudentAttendanceController(IStudentAttendanceService service) : ControllerBase
{
    private const string ManageRoles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Teacher}";

    [HttpGet]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> GetForDate([FromQuery] Guid classId, [FromQuery] Guid sectionId, [FromQuery] DateTime date, CancellationToken ct = default)
        => Ok(ApiResponse<StudentAttendanceForDateResponseDto>.Ok(await service.GetForDateAsync(classId, sectionId, date, ct), "Student attendance retrieved"));

    [HttpPatch("save")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Save(SaveStudentAttendanceDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<StudentAttendanceForDateResponseDto>.Ok(await service.SaveAsync(dto, ct), "Student attendance saved"));

    [HttpGet("report")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> GetReport([FromQuery] StudentAttendanceReportFilterDto filter, CancellationToken ct = default)
        => Ok(ApiResponse<StudentAttendanceReportResponseDto>.Ok(await service.GetReportAsync(filter, ct), "Student attendance report retrieved"));

    [HttpGet("report/my")]
    [Authorize(Roles = AppConstants.Roles.Student)]
    public async Task<IActionResult> GetMyReport([FromQuery] StudentAttendanceReportFilterDto filter, CancellationToken ct = default)
        => Ok(ApiResponse<StudentAttendanceReportResponseDto>.Ok(await service.GetMyReportAsync(filter, ct), "My attendance report retrieved"));
}
