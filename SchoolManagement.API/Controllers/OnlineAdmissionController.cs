using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.OnlineAdmission;
using SchoolManagement.BLL.DTOs.Student;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/online-admission")]
public class OnlineAdmissionController : ControllerBase
{
    private readonly IOnlineAdmissionService _service;

    public OnlineAdmissionController(IOnlineAdmissionService service)
    {
        _service = service;
    }

    // ── Public ──────────────────────────────────────────────────────────

    [HttpPost("apply")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<OnlineAdmissionResponseDto>>> Apply(
        [FromBody] SubmitOnlineAdmissionDto dto,
        [FromQuery] string? school,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(school) && string.IsNullOrWhiteSpace(dto.TenantSlug))
            dto.TenantSlug = school;

        var result = await _service.ApplyAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(Track), new { referenceNo = result.ReferenceNo, school = dto.TenantSlug },
            ApiResponse<OnlineAdmissionResponseDto>.Ok(result, "Application submitted successfully"));
    }

    [HttpGet("track/{referenceNo}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<OnlineAdmissionTrackDto>>> Track(
        string referenceNo,
        [FromQuery] string? school,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.TrackAsync(referenceNo, school, cancellationToken);
        return Ok(ApiResponse<OnlineAdmissionTrackDto>.Ok(result, "Application status retrieved"));
    }

    [HttpGet("lookup/classes/{tenantSlug}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AdmissionLookupItemDto>>>> GetPublicClasses(
        string tenantSlug,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetPublicClassesAsync(tenantSlug, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AdmissionLookupItemDto>>.Ok(result, "Classes retrieved"));
    }

    // ── Admin ───────────────────────────────────────────────────────────

    [HttpGet]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Teacher}")]
    public async Task<IActionResult> GetList(
        [FromQuery] Guid? classId,
        [FromQuery] string? status,
        [FromQuery] string? paymentStatus,
        [FromQuery] int? academicYear,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? export = null,
        CancellationToken cancellationToken = default)
    {
        var filter = new OnlineAdmissionFilterDto
        {
            ClassId = classId,
            Status = status,
            PaymentStatus = paymentStatus,
            AcademicYear = academicYear,
            Search = search,
            Page = page,
            PageSize = pageSize,
            Export = export
        };

        if (!string.IsNullOrWhiteSpace(export))
        {
            var file = await _service.ExportAsync(filter, cancellationToken);
            return File(file.Content, file.ContentType, file.FileName);
        }

        var result = await _service.GetListAsync(filter, cancellationToken);
        return Ok(ApiResponse<OnlineAdmissionListResponseDto>.Ok(result, "Applications retrieved"));
    }

    [HttpGet("export")]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
    public async Task<IActionResult> Export(
        [FromQuery] Guid? classId,
        [FromQuery] string? status,
        [FromQuery] string? paymentStatus,
        [FromQuery] int? academicYear,
        [FromQuery] string? search,
        [FromQuery] string export = "csv",
        CancellationToken cancellationToken = default)
    {
        var file = await _service.ExportAsync(new OnlineAdmissionFilterDto
        {
            ClassId = classId,
            Status = status,
            PaymentStatus = paymentStatus,
            AcademicYear = academicYear,
            Search = search,
            Export = export
        }, cancellationToken);

        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Teacher}")]
    public async Task<ActionResult<ApiResponse<OnlineAdmissionResponseDto>>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<OnlineAdmissionResponseDto>.Ok(result, "Application retrieved"));
    }

    [HttpGet("{id:guid}/print")]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Teacher}")]
    public async Task<ActionResult<ApiResponse<OnlineAdmissionResponseDto>>> Print(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetPrintDataAsync(id, cancellationToken);
        return Ok(ApiResponse<OnlineAdmissionResponseDto>.Ok(result, "Print data retrieved"));
    }

    [HttpPatch("{id:guid}/approve")]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
    public async Task<ActionResult<ApiResponse<OnlineAdmissionResponseDto>>> Approve(
        Guid id,
        [FromBody] ApproveAdmissionDto dto,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.ApproveAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<OnlineAdmissionResponseDto>.Ok(result, "Application approved"));
    }

    [HttpPatch("{id:guid}/decline")]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
    public async Task<ActionResult<ApiResponse<OnlineAdmissionResponseDto>>> Decline(
        Guid id,
        [FromBody] DeclineAdmissionDto dto,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.DeclineAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<OnlineAdmissionResponseDto>.Ok(result, "Application declined"));
    }

    [HttpPatch("{id:guid}/payment")]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
    public async Task<ActionResult<ApiResponse<OnlineAdmissionResponseDto>>> UpdatePayment(
        Guid id,
        [FromBody] UpdatePaymentStatusDto dto,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.UpdatePaymentAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<OnlineAdmissionResponseDto>.Ok(result, "Payment status updated"));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse.Ok("Application deleted"));
    }
}
