using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.Events;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/events")]
public class EventController(IEventService service) : ControllerBase
{
    private const string ManageRoles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}";

    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublic(CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<PublicEventDto>>.Ok(await service.GetPublicAsync(ct), "Public events retrieved"));

    [HttpGet]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> GetList([FromQuery] EventFilterDto filter, CancellationToken ct = default)
        => Ok(ApiResponse<EventListResponseDto>.Ok(await service.GetListAsync(filter, ct), "Events retrieved"));

    [HttpGet("{id:guid}")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct = default)
        => Ok(ApiResponse<EventDetailDto>.Ok(await service.GetByIdAsync(id, ct), "Event retrieved"));

    [HttpPost]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Create(CreateEventDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<EventDetailDto>.Ok(await service.CreateAsync(dto, ct), "Event created"));

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Update(Guid id, UpdateEventDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<EventDetailDto>.Ok(await service.UpdateAsync(id, dto, ct), "Event updated"));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await service.DeleteAsync(id, ct);
        return Ok(ApiResponse.Ok("Event deleted"));
    }

    [HttpPost("{id:guid}/image")]
    [Authorize(Roles = ManageRoles)]
    [RequestSizeLimit(3 * 1024 * 1024)]
    public async Task<IActionResult> UploadImage(Guid id, IFormFile file, CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse.Fail("File is required."));
        await using var stream = file.OpenReadStream();
        return Ok(ApiResponse<EventDetailDto>.Ok(await service.UploadImageAsync(id, stream, file.FileName, file.ContentType, ct), "Event image uploaded"));
    }

    [HttpPatch("{id:guid}/publish")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> TogglePublish(Guid id, CancellationToken ct = default)
        => Ok(ApiResponse<EventDetailDto>.Ok(await service.TogglePublishAsync(id, ct), "Event publish state updated"));

    [HttpPatch("{id:guid}/show-website")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> ToggleShowWebsite(Guid id, CancellationToken ct = default)
        => Ok(ApiResponse<EventDetailDto>.Ok(await service.ToggleShowWebsiteAsync(id, ct), "Event website visibility updated"));
}
