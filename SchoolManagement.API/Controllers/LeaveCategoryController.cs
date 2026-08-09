using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.Leave;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/leave-categories")]
[Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
public class LeaveCategoryController(ILeaveCategoryService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<LeaveCategoryResponseDto>>.Ok(await service.GetAllAsync(ct), "Leave categories retrieved"));

    [HttpGet("lookup")]
    public async Task<IActionResult> Lookup([FromQuery] string? role, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<LeaveCategoryLookupDto>>.Ok(await service.GetLookupAsync(role, ct), "Leave category lookup retrieved"));

    [HttpPost]
    public async Task<IActionResult> Create(CreateLeaveCategoryDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<LeaveCategoryResponseDto>.Ok(await service.CreateAsync(dto, ct), "Leave category created"));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateLeaveCategoryDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<LeaveCategoryResponseDto>.Ok(await service.UpdateAsync(id, dto, ct), "Leave category updated"));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await service.DeleteAsync(id, ct);
        return Ok(ApiResponse.Ok("Leave category deleted"));
    }
}
