using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.Settings;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/settings/school")]
[Authorize]
public class SchoolSettingsController(ISchoolSettingsService service) : ControllerBase
{
    private const string ManageRoles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}";

    [HttpGet]
    [Authorize(Roles = AppConstants.Roles.SuperAdmin)]
    public async Task<IActionResult> GetSchoolList(
        [FromQuery] string? search,
        [FromQuery] string? name,
        [FromQuery] string? slug,
        [FromQuery] string? city,
        [FromQuery] string? state,
        [FromQuery] string? schoolType,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var filter = new SchoolSearchFilter
        {
            Search = search,
            Name = name,
            Slug = slug,
            City = city,
            State = state,
            SchoolType = schoolType,
            IsActive = isActive,
            Page = page,
            PageSize = pageSize
        };
        return Ok(ApiResponse<SchoolListResponseDto>.Ok(await service.GetSchoolListAsync(filter, ct), "Schools retrieved"));
    }

    [HttpGet("{tenantSlug}")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> GetBySlug(string tenantSlug, CancellationToken ct = default)
        => Ok(ApiResponse<SchoolSettingsResponseDto>.Ok(await service.GetBySlugAsync(tenantSlug, ct), "School settings retrieved"));

    [HttpPatch("{tenantSlug}/general")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> UpdateGeneral(string tenantSlug, UpdateSchoolGeneralDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<SchoolSettingsResponseDto>.Ok(await service.UpdateGeneralAsync(tenantSlug, dto, ct), "General settings updated"));

    [HttpPatch("{tenantSlug}/student-panel")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> UpdateStudentPanel(string tenantSlug, UpdateStudentPanelDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<SchoolSettingsResponseDto>.Ok(await service.UpdateStudentPanelAsync(tenantSlug, dto, ct), "Student panel settings updated"));

    [HttpPatch("{tenantSlug}/payment")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> UpdatePayment(string tenantSlug, PaymentSettingsDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<SchoolSettingsResponseDto>.Ok(await service.UpdatePaymentAsync(tenantSlug, dto, ct), "Payment settings updated"));

    [HttpPost("{tenantSlug}/logo")]
    [Authorize(Roles = ManageRoles)]
    [RequestSizeLimit(5_000_000)]
    public async Task<IActionResult> UploadLogo(string tenantSlug, [FromQuery] string type, IFormFile file, CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            throw new AppException("Logo file is required.", 400);

        await using var stream = file.OpenReadStream();
        var result = await service.UploadLogoAsync(tenantSlug, type, stream, file.FileName, file.ContentType, ct);
        return Ok(ApiResponse<SchoolSettingsResponseDto>.Ok(result, "Logo uploaded"));
    }
}
