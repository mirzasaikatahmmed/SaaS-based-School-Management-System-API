using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.API.Filters;
using SchoolManagement.BLL.DTOs.Attendance;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/attendance/subject")]
[Authorize]
public class SubjectAttendanceController(ISubjectAttendanceService service) : ControllerBase
{
    [HttpGet]
    [AuthorizePermission("Attendance.StudentAttendance", AppConstants.PermissionActions.View)]
    public async Task<IActionResult> Get(
        [FromQuery] Guid classId,
        [FromQuery] Guid sectionId,
        [FromQuery] Guid subjectId,
        [FromQuery] DateTime date,
        CancellationToken ct = default)
        => Ok(ApiResponse<SubjectAttendanceForDateResponseDto>.Ok(
            await service.GetForDateAsync(classId, sectionId, subjectId, date, ct),
            "Subject attendance retrieved"));

    [HttpPatch("save")]
    [AuthorizePermission("Attendance.StudentAttendance", AppConstants.PermissionActions.Edit)]
    public async Task<IActionResult> Save(SaveSubjectAttendanceDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<SubjectAttendanceForDateResponseDto>.Ok(
            await service.SaveAsync(dto, ct),
            "Subject attendance saved"));
}
