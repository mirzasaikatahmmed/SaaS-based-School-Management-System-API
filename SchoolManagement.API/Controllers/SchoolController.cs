using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.School;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;
using SchoolManagement.DAL.Repositories.Interfaces;
using SchoolManagement.DAL.TenantContext;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/schools")]
[Authorize]
public class SchoolController : ControllerBase
{
    private readonly ISchoolService _schoolService;
    private readonly ITenantContext _tenantContext;

    public SchoolController(ISchoolService schoolService, ITenantContext tenantContext)
    {
        _schoolService = schoolService;
        _tenantContext = tenantContext;
    }

    /// <summary>Paginated school list — Super Admin only. Supports ?export=csv|excel|pdf</summary>
    [HttpGet]
    [Authorize(Roles = AppConstants.Roles.SuperAdmin)]
    public async Task<IActionResult> GetSchools(
        [FromQuery] string? search,
        [FromQuery] string? name,
        [FromQuery] string? slug,
        [FromQuery] string? city,
        [FromQuery] string? state,
        [FromQuery] string? schoolType,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? export = null,
        CancellationToken cancellationToken = default)
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

        if (!string.IsNullOrWhiteSpace(export))
        {
            var (content, contentType, fileName) =
                await _schoolService.ExportAsync(filter, export, cancellationToken);
            return File(content, contentType, fileName);
        }

        var result = await _schoolService.GetSchoolsAsync(filter, cancellationToken);
        return Ok(ApiResponse<SchoolListResponseDto>.Ok(result, "Schools retrieved"));
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<ApiResponse<SchoolResponseDto>>> GetBySlug(
        string slug,
        CancellationToken cancellationToken)
    {
        EnsureCanAccessSchool(slug);
        var result = await _schoolService.GetBySlugAsync(slug, cancellationToken);
        return Ok(ApiResponse<SchoolResponseDto>.Ok(result, "School retrieved"));
    }

    [HttpPost]
    [Authorize(Roles = AppConstants.Roles.SuperAdmin)]
    public async Task<ActionResult<ApiResponse<SchoolResponseDto>>> Create(
        [FromBody] CreateSchoolDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _schoolService.CreateSchoolAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetBySlug), new { slug = result.Slug },
            ApiResponse<SchoolResponseDto>.Ok(result, "School created successfully"));
    }

    [HttpPut("{slug}")]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
    public async Task<ActionResult<ApiResponse<SchoolResponseDto>>> Update(
        string slug,
        [FromBody] UpdateSchoolDto dto,
        CancellationToken cancellationToken)
    {
        EnsureCanAccessSchool(slug);
        var result = await _schoolService.UpdateSchoolAsync(slug, dto, cancellationToken);
        return Ok(ApiResponse<SchoolResponseDto>.Ok(result, "School updated"));
    }

    [HttpDelete("{slug}")]
    [Authorize(Roles = AppConstants.Roles.SuperAdmin)]
    public async Task<ActionResult<ApiResponse>> Deactivate(
        string slug,
        CancellationToken cancellationToken)
    {
        await _schoolService.DeactivateAsync(slug, cancellationToken);
        return Ok(ApiResponse.Ok("School deactivated"));
    }

    [HttpPut("{slug}/activate")]
    [Authorize(Roles = AppConstants.Roles.SuperAdmin)]
    public async Task<ActionResult<ApiResponse>> Activate(
        string slug,
        CancellationToken cancellationToken)
    {
        await _schoolService.ActivateAsync(slug, cancellationToken);
        return Ok(ApiResponse.Ok("School activated"));
    }

    [HttpPost("{slug}/logo")]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
    [RequestSizeLimit(5_000_000)]
    public async Task<ActionResult<ApiResponse<SchoolResponseDto>>> UploadLogo(
        string slug,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        EnsureCanAccessSchool(slug);
        if (file is null || file.Length == 0)
            throw new AppException("Logo file is required.", 400);

        await using var stream = file.OpenReadStream();
        var result = await _schoolService.UploadLogoAsync(
            slug, stream, file.FileName, file.ContentType, cancellationToken);
        return Ok(ApiResponse<SchoolResponseDto>.Ok(result, "Logo uploaded"));
    }

    [HttpGet("{slug}/settings")]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
    public async Task<ActionResult<ApiResponse<SchoolSettingsDto>>> GetSettings(
        string slug,
        CancellationToken cancellationToken)
    {
        EnsureCanAccessSchool(slug);
        var result = await _schoolService.GetSettingsAsync(slug, cancellationToken);
        return Ok(ApiResponse<SchoolSettingsDto>.Ok(result, "Settings retrieved"));
    }

    [HttpPut("{slug}/settings")]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
    public async Task<ActionResult<ApiResponse<SchoolSettingsDto>>> UpdateSettings(
        string slug,
        [FromBody] SchoolSettingsDto dto,
        CancellationToken cancellationToken)
    {
        EnsureCanAccessSchool(slug);
        var result = await _schoolService.UpdateSettingsAsync(slug, dto, cancellationToken);
        return Ok(ApiResponse<SchoolSettingsDto>.Ok(result, "Settings updated"));
    }

    [HttpGet("{slug}/stats")]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
    public async Task<ActionResult<ApiResponse<SchoolStatsDto>>> GetStats(
        string slug,
        CancellationToken cancellationToken)
    {
        EnsureCanAccessSchool(slug);
        var result = await _schoolService.GetStatsAsync(slug, cancellationToken);
        return Ok(ApiResponse<SchoolStatsDto>.Ok(result, "Stats retrieved"));
    }

    private void EnsureCanAccessSchool(string slug)
    {
        if (User.IsInRole(AppConstants.Roles.SuperAdmin))
            return;

        if (User.IsInRole(AppConstants.Roles.Admin))
        {
            if (string.IsNullOrEmpty(_tenantContext.TenantSlug))
                throw new ForbiddenException("X-Tenant-ID header is required for school admin access.");

            if (!string.Equals(_tenantContext.TenantSlug, slug, StringComparison.OrdinalIgnoreCase))
                throw new ForbiddenException("You can only access your own school.");

            return;
        }

        throw new ForbiddenException("You do not have permission to access schools.");
    }
}
