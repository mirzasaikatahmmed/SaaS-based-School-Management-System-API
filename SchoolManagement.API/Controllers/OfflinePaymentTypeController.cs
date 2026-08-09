using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.StudentAccounting;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/student-accounting/offline-payment-types")]
[Authorize]
public class OfflinePaymentTypeController(IOfflinePaymentTypeService service) : ControllerBase
{
    private const string ManageRoles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Accountant}";

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<OfflinePaymentTypeResponseDto>>.Ok(await service.GetAllAsync(ct), "Payment types retrieved"));

    [HttpGet("{id:guid}")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
        => Ok(ApiResponse<OfflinePaymentTypeResponseDto>.Ok(await service.GetByIdAsync(id, ct), "Payment type retrieved"));

    [HttpPost]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Create(CreateOfflinePaymentTypeDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<OfflinePaymentTypeResponseDto>.Ok(await service.CreateAsync(dto, ct), "Payment type created"));

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Update(Guid id, UpdateOfflinePaymentTypeDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<OfflinePaymentTypeResponseDto>.Ok(await service.UpdateAsync(id, dto, ct), "Payment type updated"));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await service.DeleteAsync(id, ct);
        return Ok(ApiResponse.Ok("Payment type deleted"));
    }
}
