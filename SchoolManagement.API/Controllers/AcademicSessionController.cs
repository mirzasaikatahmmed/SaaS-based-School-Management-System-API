using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.API.Filters;
using SchoolManagement.BLL.DTOs.Settings;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/sessions")]
[Authorize]
public class AcademicSessionController(IAcademicSessionService service) : ControllerBase
{
    [HttpGet]
    [AuthorizePermission("Settings.AcademicSessions", AppConstants.PermissionActions.View)]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<AcademicSessionResponseDto>>.Ok(await service.GetAllAsync(ct), "Sessions retrieved"));

    [HttpGet("current")]
    [AuthorizePermission("Settings.AcademicSessions", AppConstants.PermissionActions.View)]
    public async Task<IActionResult> GetCurrent(CancellationToken ct = default)
        => Ok(ApiResponse<AcademicSessionResponseDto?>.Ok(await service.GetCurrentAsync(ct), "Current session retrieved"));

    [HttpPost]
    [AuthorizePermission("Settings.AcademicSessions", AppConstants.PermissionActions.Add)]
    public async Task<IActionResult> Create(CreateAcademicSessionDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<AcademicSessionResponseDto>.Ok(await service.CreateAsync(dto, ct), "Session created"));

    [HttpPatch("{id:guid}")]
    [AuthorizePermission("Settings.AcademicSessions", AppConstants.PermissionActions.Edit)]
    public async Task<IActionResult> Update(Guid id, UpdateAcademicSessionDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<AcademicSessionResponseDto>.Ok(await service.UpdateAsync(id, dto, ct), "Session updated"));

    [HttpDelete("{id:guid}")]
    [AuthorizePermission("Settings.AcademicSessions", AppConstants.PermissionActions.Delete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await service.DeleteAsync(id, ct);
        return Ok(ApiResponse.Ok("Session deleted"));
    }
}
