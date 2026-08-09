using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.OfficeAccounting;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/office-accounting/transactions")]
[Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Accountant}")]
public class OfficeAccountingTransactionsController(IAccountingAccountService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTransactions([FromQuery] TransactionFilterDto filter, CancellationToken ct = default)
        => Ok(ApiResponse<TransactionListResponseDto>.Ok(await service.GetTransactionsAsync(filter, ct), "Transactions retrieved"));
}
