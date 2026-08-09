using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.Payroll;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/payroll/salary-payment")]
[Authorize]
public class SalaryPaymentController(ISalaryPaymentService service) : ControllerBase
{
    private const string ManageRoles =
        $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Accountant}";

    [HttpGet]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> GetList([FromQuery] SalaryPaymentFilterDto filter, CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(filter.Export))
        {
            var file = await service.ExportAsync(filter, ct);
            return File(file.Content, file.ContentType, file.FileName);
        }

        return Ok(ApiResponse<SalaryPaymentListResponseDto>.Ok(await service.GetListAsync(filter, ct), "Salary payments retrieved"));
    }

    [HttpGet("export")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Export([FromQuery] SalaryPaymentFilterDto filter, CancellationToken ct = default)
    {
        filter.Export ??= "csv";
        var file = await service.ExportAsync(filter, ct);
        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct = default)
        => Ok(ApiResponse<SalaryPaymentResponseDto>.Ok(await service.GetByIdAsync(id, ct), "Salary payment retrieved"));

    [HttpPost("{employeeId:guid}/pay")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Pay(Guid employeeId, ProcessPaymentDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<SalaryPaymentResponseDto>.Ok(await service.ProcessPaymentAsync(employeeId, dto, ct), "Salary payment processed"));

    [HttpPut("{id:guid}/update")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Update(Guid id, ProcessPaymentDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<SalaryPaymentResponseDto>.Ok(await service.UpdatePaymentAsync(id, dto, ct), "Salary payment updated"));

    [HttpGet("~/api/payroll/my-salary")]
    public async Task<IActionResult> MySalary(CancellationToken ct = default)
        => Ok(ApiResponse<MySalaryDto>.Ok(await service.GetMySalaryAsync(ct), "My salary retrieved"));

    [HttpGet("~/api/payroll/my-salary/{month}")]
    public async Task<IActionResult> MySalaryMonth(string month, CancellationToken ct = default)
        => Ok(ApiResponse<SalaryPaymentResponseDto>.Ok(await service.GetMySalaryForMonthAsync(month, ct), "My salary for month retrieved"));
}
