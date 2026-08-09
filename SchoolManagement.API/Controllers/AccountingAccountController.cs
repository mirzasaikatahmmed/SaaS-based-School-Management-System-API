using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.OfficeAccounting;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/office-accounting/accounts")]
[Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Accountant}")]
public class AccountingAccountController(IAccountingAccountService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<AccountingAccountResponseDto>>.Ok(await service.GetAllAsync(isActive, ct), "Accounts retrieved"));

    [HttpGet("lookup")]
    public async Task<IActionResult> Lookup(CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<AccountingAccountLookupDto>>.Ok(await service.GetLookupAsync(ct), "Accounts retrieved"));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
        => Ok(ApiResponse<AccountingAccountResponseDto>.Ok(await service.GetByIdAsync(id, ct), "Account retrieved"));

    [HttpPost]
    public async Task<IActionResult> Create(CreateAccountingAccountDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<AccountingAccountResponseDto>.Ok(await service.CreateAsync(dto, ct), "Account created"));

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateAccountingAccountDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<AccountingAccountResponseDto>.Ok(await service.UpdateAsync(id, dto, ct), "Account updated"));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await service.DeleteAsync(id, ct);
        return Ok(ApiResponse.Ok("Account deleted"));
    }
}
