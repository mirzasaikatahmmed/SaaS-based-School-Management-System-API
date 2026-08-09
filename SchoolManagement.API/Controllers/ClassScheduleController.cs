using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.Academic;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/academic/class-schedules")]
[Authorize]
public class ClassScheduleController(IClassScheduleService service) : ControllerBase
{
    private const string ManageRoles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}";
    private const string ReadRoles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Teacher},{AppConstants.Roles.Student}";

    [HttpGet("by-class-section")]
    [Authorize(Roles = ReadRoles)]
    public async Task<IActionResult> GetByClassSection([FromQuery] Guid classId, [FromQuery] Guid sectionId, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<ClassScheduleResponseDto>>.Ok(await service.GetByClassSectionAsync(classId, sectionId, ct), "Class schedule retrieved"));

    [HttpGet("my")]
    [Authorize(Roles = AppConstants.Roles.Student)]
    public async Task<IActionResult> GetMy(CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<ClassScheduleResponseDto>>.Ok(await service.GetMyClassScheduleAsync(ct), "My class schedule retrieved"));

    [HttpPost]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Upsert(UpsertClassScheduleDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<ClassScheduleResponseDto>.Ok(await service.UpsertAsync(dto, ct), "Class schedule saved"));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await service.DeleteAsync(id, ct);
        return Ok(ApiResponse.Ok("Class schedule deleted"));
    }
}
