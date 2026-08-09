using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.Academic;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/academic/sections")]
[Authorize]
public class SectionControlController(ISectionControlService service) : ControllerBase
{
    private const string ManageRoles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}";
    private const string ReadRoles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Teacher},{AppConstants.Roles.Student}";

    [HttpGet]
    [Authorize(Roles = ReadRoles)]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<SectionResponseDto>>.Ok(await service.GetAllAsync(ct), "Sections retrieved"));

    [HttpGet("{id:guid}")]
    [Authorize(Roles = ReadRoles)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
        => Ok(ApiResponse<SectionResponseDto>.Ok(await service.GetByIdAsync(id, ct), "Section retrieved"));

    [HttpPost]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Create(CreateSectionDto dto, CancellationToken ct = default)
    {
        var result = await service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<SectionResponseDto>.Ok(result, "Section created"));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Update(Guid id, UpdateSectionDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<SectionResponseDto>.Ok(await service.UpdateAsync(id, dto, ct), "Section updated"));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await service.DeleteAsync(id, ct);
        return Ok(ApiResponse.Ok("Section deleted"));
    }
}
