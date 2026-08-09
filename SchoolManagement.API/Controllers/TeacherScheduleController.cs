using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.Academic;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/academic/teacher-schedule")]
[Authorize]
public class TeacherScheduleController(IClassScheduleService service) : ControllerBase
{
    [HttpGet("me")]
    [Authorize(Roles = AppConstants.Roles.Teacher)]
    public async Task<IActionResult> GetMy(CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<TeacherScheduleDayDto>>.Ok(await service.GetMyTeacherScheduleAsync(ct), "My teaching schedule retrieved"));

    [HttpGet("{employeeId:guid}")]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Teacher}")]
    public async Task<IActionResult> GetByEmployee(Guid employeeId, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<TeacherScheduleDayDto>>.Ok(await service.GetTeacherScheduleAsync(employeeId, ct), "Teacher schedule retrieved"));
}
