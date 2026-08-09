using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.Award;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/awards")]
[Authorize]
public class AwardController(IAwardService service) : ControllerBase
{
    private const string ManageRoles =
        $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Accountant}";

    [HttpGet]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> GetList([FromQuery] AwardFilterDto filter, CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(filter.Export))
        {
            var file = await service.ExportAsync(filter, ct);
            return File(file.Content, file.ContentType, file.FileName);
        }

        return Ok(ApiResponse<AwardListResponseDto>.Ok(await service.GetListAsync(filter, ct), "Awards retrieved"));
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMy([FromQuery] AwardFilterDto filter, CancellationToken ct = default)
        => Ok(ApiResponse<AwardListResponseDto>.Ok(await service.GetMyAwardsAsync(filter, ct), "My awards retrieved"));

    [HttpGet("export")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Export([FromQuery] AwardFilterDto filter, CancellationToken ct = default)
    {
        filter.Export ??= "csv";
        var file = await service.ExportAsync(filter, ct);
        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("lookup/winners")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Winners([FromQuery] string role, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<WinnerLookupDto>>.Ok(await service.GetWinnersLookupAsync(role, ct), "Winners retrieved"));

    [HttpPost]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Give(GiveAwardDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<AwardResponseDto>.Ok(await service.GiveAwardAsync(dto, ct), "Award given"));

    [HttpPut("{id:guid}")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Update(Guid id, UpdateAwardDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<AwardResponseDto>.Ok(await service.UpdateAsync(id, dto, ct), "Award updated"));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await service.DeleteAsync(id, ct);
        return Ok(ApiResponse.Ok("Award deleted"));
    }
}
