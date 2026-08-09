using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.OfficeAccounting;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/office-accounting/deposits")]
[Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Accountant}")]
public class AccountingDepositController(IAccountingDepositService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetFiltered([FromQuery] AccountingDepositFilterDto filter, CancellationToken ct = default)
        => Ok(ApiResponse<AccountingDepositListResponseDto>.Ok(await service.GetFilteredAsync(filter, ct), "Deposits retrieved"));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
        => Ok(ApiResponse<AccountingDepositResponseDto>.Ok(await service.GetByIdAsync(id, ct), "Deposit retrieved"));

    [HttpPost]
    public async Task<IActionResult> Create(CreateAccountingDepositDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<AccountingDepositResponseDto>.Ok(await service.CreateAsync(dto, ct), "Deposit created"));

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateAccountingDepositDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<AccountingDepositResponseDto>.Ok(await service.UpdateAsync(id, dto, ct), "Deposit updated"));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await service.DeleteAsync(id, ct);
        return Ok(ApiResponse.Ok("Deposit deleted"));
    }

    [HttpPost("{id:guid}/attachment")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> Attachment(Guid id, IFormFile file, CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse.Fail("File is required."));
        await using var stream = file.OpenReadStream();
        return Ok(ApiResponse<AccountingDepositResponseDto>.Ok(
            await service.UploadAttachmentAsync(id, stream, file.FileName, file.ContentType, ct),
            "Deposit attachment uploaded"));
    }
}
