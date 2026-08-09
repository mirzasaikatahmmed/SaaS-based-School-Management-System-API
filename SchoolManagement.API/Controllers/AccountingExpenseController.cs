using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.OfficeAccounting;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/office-accounting/expenses")]
[Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Accountant}")]
public class AccountingExpenseController(IAccountingExpenseService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetFiltered([FromQuery] AccountingExpenseFilterDto filter, CancellationToken ct = default)
        => Ok(ApiResponse<AccountingExpenseListResponseDto>.Ok(await service.GetFilteredAsync(filter, ct), "Expenses retrieved"));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
        => Ok(ApiResponse<AccountingExpenseResponseDto>.Ok(await service.GetByIdAsync(id, ct), "Expense retrieved"));

    [HttpPost]
    public async Task<IActionResult> Create(CreateAccountingExpenseDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<AccountingExpenseResponseDto>.Ok(await service.CreateAsync(dto, ct), "Expense created"));

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateAccountingExpenseDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<AccountingExpenseResponseDto>.Ok(await service.UpdateAsync(id, dto, ct), "Expense updated"));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await service.DeleteAsync(id, ct);
        return Ok(ApiResponse.Ok("Expense deleted"));
    }

    [HttpPost("{id:guid}/attachment")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> Attachment(Guid id, IFormFile file, CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse.Fail("File is required."));
        await using var stream = file.OpenReadStream();
        return Ok(ApiResponse<AccountingExpenseResponseDto>.Ok(
            await service.UploadAttachmentAsync(id, stream, file.FileName, file.ContentType, ct),
            "Expense attachment uploaded"));
    }
}
