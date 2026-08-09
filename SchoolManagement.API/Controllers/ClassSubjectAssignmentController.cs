using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.Academic;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/academic/class-subject-assignments")]
[Authorize]
public class ClassSubjectAssignmentController(IClassSubjectAssignmentService service) : ControllerBase
{
    private const string ManageRoles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}";
    private const string ReadRoles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Teacher},{AppConstants.Roles.Student}";

    [HttpGet]
    [Authorize(Roles = ReadRoles)]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<ClassSubjectAssignmentResponseDto>>.Ok(await service.GetAllAsync(ct), "Subject assignments retrieved"));

    [HttpGet("by-class-section")]
    [Authorize(Roles = ReadRoles)]
    public async Task<IActionResult> GetByClassSection([FromQuery] Guid classId, [FromQuery] Guid sectionId, CancellationToken ct = default)
        => Ok(ApiResponse<ClassSubjectAssignmentResponseDto>.Ok(await service.GetByClassSectionAsync(classId, sectionId, ct), "Subject assignment retrieved"));

    [HttpPost]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Upsert(UpsertClassSubjectAssignmentDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<ClassSubjectAssignmentResponseDto>.Ok(await service.UpsertAsync(dto, ct), "Subject assignment saved"));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await service.DeleteAsync(id, ct);
        return Ok(ApiResponse.Ok("Subject assignment deleted"));
    }
}
