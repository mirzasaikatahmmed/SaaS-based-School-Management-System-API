using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.OfficeAccounting;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/office-accounting/voucher-heads")]
[Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Accountant}")]
public class VoucherHeadController(IVoucherHeadService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? type, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<VoucherHeadResponseDto>>.Ok(await service.GetAllAsync(type, ct), "Voucher heads retrieved"));

    /// <summary>Lightweight lookup for dropdowns — same data as GetAll, filterable by type (Income/Expense).</summary>
    [HttpGet("lookup")]
    public async Task<IActionResult> Lookup([FromQuery] string? type, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<VoucherHeadResponseDto>>.Ok(await service.GetAllAsync(type, ct), "Voucher head lookup retrieved"));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
        => Ok(ApiResponse<VoucherHeadResponseDto>.Ok(await service.GetByIdAsync(id, ct), "Voucher head retrieved"));

    [HttpPost]
    public async Task<IActionResult> Create(CreateVoucherHeadDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<VoucherHeadResponseDto>.Ok(await service.CreateAsync(dto, ct), "Voucher head created"));

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateVoucherHeadDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<VoucherHeadResponseDto>.Ok(await service.UpdateAsync(id, dto, ct), "Voucher head updated"));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await service.DeleteAsync(id, ct);
        return Ok(ApiResponse.Ok("Voucher head deleted"));
    }
}
