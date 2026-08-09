using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.StudentAccounting;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/student-accounting/offline-payments")]
[Authorize]
public class OfflinePaymentController(IOfflinePaymentService service) : ControllerBase
{
    private const string ManageRoles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Accountant}";
    private const string SubmitRoles = $"{AppConstants.Roles.Student},{AppConstants.Roles.Parent},{AppConstants.Roles.Admin},{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Accountant}";

    [HttpPost]
    [Authorize(Roles = SubmitRoles)]
    public async Task<IActionResult> Submit(CreateOfflinePaymentDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<OfflinePaymentResponseDto>.Ok(await service.SubmitAsync(dto, ct), "Offline payment submitted"));

    [HttpGet]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> GetFiltered([FromQuery] OfflinePaymentFilterDto filter, CancellationToken ct = default)
        => Ok(ApiResponse<OfflinePaymentListResponseDto>.Ok(await service.GetFilteredAsync(filter, ct), "Offline payments retrieved"));

    [HttpGet("my")]
    [Authorize(Roles = $"{AppConstants.Roles.Student},{AppConstants.Roles.Parent}")]
    public async Task<IActionResult> GetMy(CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<OfflinePaymentResponseDto>>.Ok(await service.GetMyPaymentsAsync(ct), "My offline payments retrieved"));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
        => Ok(ApiResponse<OfflinePaymentResponseDto>.Ok(await service.GetByIdAsync(id, ct), "Offline payment retrieved"));

    [HttpPatch("{id:guid}/approve")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Approve(Guid id, ReviewOfflinePaymentDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<OfflinePaymentResponseDto>.Ok(await service.ApproveAsync(id, dto, ct), "Offline payment approved"));

    [HttpPatch("{id:guid}/reject")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Reject(Guid id, ReviewOfflinePaymentDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<OfflinePaymentResponseDto>.Ok(await service.RejectAsync(id, dto, ct), "Offline payment rejected"));
}
