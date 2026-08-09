using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.StudentAccounting;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/student-accounting/fees-allocations")]
[Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Accountant}")]
public class FeesAllocationController(IFeesAllocationService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetFiltered([FromQuery] FeesAllocationFilterDto filter, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<FeesAllocationResponseDto>>.Ok(await service.GetFilteredAsync(filter, ct), "Fees allocations retrieved"));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
        => Ok(ApiResponse<FeesAllocationResponseDto>.Ok(await service.GetByIdAsync(id, ct), "Fees allocation retrieved"));

    [HttpPost]
    public async Task<IActionResult> Create(CreateFeesAllocationDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<FeesAllocationResponseDto>.Ok(await service.CreateAsync(dto, ct), "Fees allocation created and invoices generated"));

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateFeesAllocationDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<FeesAllocationResponseDto>.Ok(await service.UpdateAsync(id, dto, ct), "Fees allocation updated"));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await service.DeleteAsync(id, ct);
        return Ok(ApiResponse.Ok("Fees allocation deleted"));
    }

    [HttpPost("{id:guid}/generate-invoices")]
    public async Task<IActionResult> GenerateInvoices(Guid id, CancellationToken ct = default)
        => Ok(ApiResponse<GenerateInvoicesResultDto>.Ok(await service.GenerateInvoicesForAllocationAsync(id, ct), "Invoices generated"));
}
