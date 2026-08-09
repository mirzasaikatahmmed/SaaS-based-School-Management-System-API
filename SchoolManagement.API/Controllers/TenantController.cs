using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.Tenant;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/tenants")]
public class TenantController : ControllerBase
{
    private readonly ITenantService _tenantService;

    public TenantController(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    [HttpPost]
    [Authorize(Roles = AppConstants.Roles.SuperAdmin)]
    public async Task<ActionResult<ApiResponse<TenantResponseDto>>> Create(
        [FromBody] CreateTenantDto request,
        CancellationToken cancellationToken)
    {
        var result = await _tenantService.CreateTenantAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetBySlug), new { slug = result.Slug },
            ApiResponse<TenantResponseDto>.Ok(result, "Tenant created successfully"));
    }

    [HttpGet]
    [Authorize(Roles = AppConstants.Roles.SuperAdmin)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TenantResponseDto>>>> GetAll(
        CancellationToken cancellationToken)
    {
        var result = await _tenantService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<TenantResponseDto>>.Ok(result, "Tenants retrieved"));
    }

    [HttpGet("{slug}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<TenantResponseDto>>> GetBySlug(
        string slug,
        CancellationToken cancellationToken)
    {
        var result = await _tenantService.GetBySlugAsync(slug, cancellationToken);
        return Ok(ApiResponse<TenantResponseDto>.Ok(result, "Tenant retrieved"));
    }

    [HttpPatch("{slug}/settings")]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
    public async Task<ActionResult<ApiResponse<TenantResponseDto>>> UpdateSettings(
        string slug,
        [FromBody] UpdateTenantSettingsDto request,
        CancellationToken cancellationToken)
    {
        var result = await _tenantService.UpdateSettingsAsync(slug, request, cancellationToken);
        return Ok(ApiResponse<TenantResponseDto>.Ok(result, "Tenant settings updated"));
    }

    [HttpDelete("{slug}")]
    [Authorize(Roles = AppConstants.Roles.SuperAdmin)]
    public async Task<ActionResult<ApiResponse>> Deactivate(
        string slug,
        CancellationToken cancellationToken)
    {
        await _tenantService.DeactivateAsync(slug, cancellationToken);
        return Ok(ApiResponse.Ok("Tenant deactivated"));
    }
}
