using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.StudentDetails;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/login-deactivate")]
[Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
public class LoginDeactivateController : ControllerBase
{
    private readonly ILoginDeactivateService _service;

    public LoginDeactivateController(ILoginDeactivateService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] LoginDeactivateFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(filter.Export))
        {
            var file = await _service.ExportAsync(filter, cancellationToken);
            return File(file.Content, file.ContentType, file.FileName);
        }

        var result = await _service.GetListAsync(filter, cancellationToken);
        return Ok(ApiResponse<LoginDeactivateListResponseDto>.Ok(result, "Login-deactivated students retrieved"));
    }

    [HttpPut("{studentId:guid}/activate")]
    public async Task<ActionResult<ApiResponse<object>>> Activate(
        Guid studentId,
        CancellationToken cancellationToken = default)
    {
        await _service.ActivateAsync(studentId, cancellationToken);
        return Ok(ApiResponse.Ok("Student login activated"));
    }

    [HttpPut("{studentId:guid}/deactivate")]
    public async Task<ActionResult<ApiResponse<object>>> Deactivate(
        Guid studentId,
        CancellationToken cancellationToken = default)
    {
        await _service.DeactivateAsync(studentId, cancellationToken);
        return Ok(ApiResponse.Ok("Student login deactivated"));
    }

    [HttpPost("bulk-activate")]
    public async Task<ActionResult<ApiResponse<BulkAuthenticationActivateResultDto>>> BulkActivate(
        [FromBody] BulkAuthenticationActivateDto dto,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.BulkActivateAsync(dto, cancellationToken);
        return Ok(ApiResponse<BulkAuthenticationActivateResultDto>.Ok(result, "Bulk authentication activate completed"));
    }
}
