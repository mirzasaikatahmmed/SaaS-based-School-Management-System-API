using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.Attendance;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/attendance/exam")]
[Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Teacher}")]
public class ExamAttendanceController(IExamAttendanceService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ExamAttendanceFilterDto filter, CancellationToken ct = default)
        => Ok(ApiResponse<ExamAttendanceResponseDto>.Ok(await service.GetAsync(filter, ct), "Exam attendance retrieved"));

    [HttpPatch("save")]
    public async Task<IActionResult> Save(SaveExamAttendanceDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<ExamAttendanceResponseDto>.Ok(await service.SaveAsync(dto, ct), "Exam attendance saved"));
}
