using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.StudentDetails;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/deactivate-reasons")]
[Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
public class DeactivateReasonController : ControllerBase
{
    private readonly IDeactivateReasonService _service;

    public DeactivateReasonController(IDeactivateReasonService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DeactivateReasonDto>>>> GetAll(
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<DeactivateReasonDto>>.Ok(result, "Deactivate reasons retrieved"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<DeactivateReasonDto>>> Create(
        [FromBody] CreateDeactivateReasonDto dto,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetAll), null,
            ApiResponse<DeactivateReasonDto>.Ok(result, "Deactivate reason created"));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<DeactivateReasonDto>>> Update(
        Guid id,
        [FromBody] UpdateDeactivateReasonDto dto,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.UpdateAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<DeactivateReasonDto>.Ok(result, "Deactivate reason updated"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse.Ok("Deactivate reason deleted"));
    }
}
