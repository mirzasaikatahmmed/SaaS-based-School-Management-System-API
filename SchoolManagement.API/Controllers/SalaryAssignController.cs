using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.Payroll;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/payroll/salary-assign")]
[Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Accountant}")]
public class SalaryAssignController(ISalaryAssignService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] SalaryAssignFilterDto filter, CancellationToken ct = default)
        => Ok(ApiResponse<SalaryAssignListResponseDto>.Ok(await service.GetListAsync(filter, ct), "Salary assignments retrieved"));

    [HttpPut("{employeeId:guid}")]
    public async Task<IActionResult> Assign(Guid employeeId, AssignSalaryGradeDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<SalaryAssignItemDto>.Ok(await service.AssignAsync(employeeId, dto, ct), "Salary grade assigned"));

    [HttpPost("bulk")]
    public async Task<IActionResult> Bulk(BulkAssignSalaryDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<BulkAssignSalaryResultDto>.Ok(await service.BulkAssignAsync(dto, ct), "Bulk salary assign completed"));
}
