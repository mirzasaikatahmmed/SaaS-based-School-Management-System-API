using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.Parents;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/parents")]
[Authorize]
public class ParentController : ControllerBase
{
    private readonly IParentService _service;

    public ParentController(IParentService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Teacher}")]
    public async Task<ActionResult<ApiResponse<ParentListResponseDto>>> GetList(
        [FromQuery] ParentListFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetListAsync(filter, cancellationToken);
        return Ok(ApiResponse<ParentListResponseDto>.Ok(result, "Parents retrieved"));
    }

    [HttpGet("me")]
    [Authorize(Roles = AppConstants.Roles.Parent)]
    public async Task<ActionResult<ApiResponse<ParentDetailDto>>> GetMe(
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetMeAsync(cancellationToken);
        return Ok(ApiResponse<ParentDetailDto>.Ok(result, "Parent profile retrieved"));
    }

    [HttpGet("export")]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
    public async Task<IActionResult> Export(
        [FromQuery] ParentListFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var file = await _service.ExportAsync(filter, cancellationToken);
        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Teacher},{AppConstants.Roles.Parent}")]
    public async Task<ActionResult<ApiResponse<ParentDetailDto>>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<ParentDetailDto>.Ok(result, "Parent retrieved"));
    }

    [HttpPost]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
    public async Task<ActionResult<ApiResponse<ParentDetailDto>>> Create(
        [FromBody] AddParentDto dto,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<ParentDetailDto>.Ok(result, "Parent created"));
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
    public async Task<ActionResult<ApiResponse<ParentDetailDto>>> Update(
        Guid id,
        [FromBody] UpdateParentDto dto,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.UpdateAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<ParentDetailDto>.Ok(result, "Parent updated"));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _service.SoftDeleteAsync(id, cancellationToken);
        return Ok(ApiResponse.Ok("Parent deleted"));
    }

    [HttpPost("{id:guid}/photo")]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
    [RequestSizeLimit(3 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<ParentDetailDto>>> UploadPhoto(
        Guid id,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse.Fail("File is required."));

        await using var stream = file.OpenReadStream();
        var result = await _service.UploadPhotoAsync(id, stream, file.FileName, file.ContentType, cancellationToken);
        return Ok(ApiResponse<ParentDetailDto>.Ok(result, "Parent photo uploaded"));
    }
}
