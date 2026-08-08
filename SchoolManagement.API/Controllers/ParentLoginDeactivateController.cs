using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.Parents;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/parent-login-deactivate")]
[Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
public class ParentLoginDeactivateController : ControllerBase
{
    private readonly IParentLoginDeactivateService _service;

    public ParentLoginDeactivateController(IParentLoginDeactivateService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] ParentLoginDeactivateFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(filter.Export))
        {
            var file = await _service.ExportAsync(filter, cancellationToken);
            return File(file.Content, file.ContentType, file.FileName);
        }

        var result = await _service.GetListAsync(filter, cancellationToken);
        return Ok(ApiResponse<ParentLoginDeactivateListResponseDto>.Ok(result, "Login-deactivated parents retrieved"));
    }

    [HttpPut("{id:guid}/activate")]
    public async Task<ActionResult<ApiResponse<object>>> Activate(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _service.ActivateAsync(id, cancellationToken);
        return Ok(ApiResponse.Ok("Parent login activated"));
    }

    [HttpPut("{id:guid}/deactivate")]
    public async Task<ActionResult<ApiResponse<object>>> Deactivate(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _service.DeactivateAsync(id, cancellationToken);
        return Ok(ApiResponse.Ok("Parent login deactivated"));
    }

    [HttpPost("bulk-activate")]
    public async Task<ActionResult<ApiResponse<BulkParentLoginActivateResultDto>>> BulkActivate(
        [FromBody] BulkParentLoginActivateDto dto,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.BulkActivateAsync(dto, cancellationToken);
        return Ok(ApiResponse<BulkParentLoginActivateResultDto>.Ok(result, "Bulk activate completed"));
    }
}
