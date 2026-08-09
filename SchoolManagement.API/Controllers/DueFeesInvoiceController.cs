using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.StudentAccounting;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/student-accounting/due-invoices")]
[Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Accountant}")]
public class DueFeesInvoiceController(IStudentFeeInvoiceService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetDue([FromQuery] DueInvoiceFilterDto filter, CancellationToken ct = default)
        => Ok(ApiResponse<StudentFeeInvoiceListResponseDto>.Ok(await service.GetDueAsync(filter, ct), "Due invoices retrieved"));
}
