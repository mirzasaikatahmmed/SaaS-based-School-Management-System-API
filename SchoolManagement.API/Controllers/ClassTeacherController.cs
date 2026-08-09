using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.Academic;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/academic/class-teachers")]
[Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
public class ClassTeacherController(IClassTeacherService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<ClassTeacherResponseDto>>.Ok(await service.GetAllAsync(ct), "Class teachers retrieved"));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
        => Ok(ApiResponse<ClassTeacherResponseDto>.Ok(await service.GetByIdAsync(id, ct), "Class teacher retrieved"));

    [HttpPost]
    public async Task<IActionResult> Upsert(UpsertClassTeacherDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<ClassTeacherResponseDto>.Ok(await service.UpsertAsync(dto, ct), "Class teacher assigned"));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await service.DeleteAsync(id, ct);
        return Ok(ApiResponse.Ok("Class teacher allocation removed"));
    }
}
