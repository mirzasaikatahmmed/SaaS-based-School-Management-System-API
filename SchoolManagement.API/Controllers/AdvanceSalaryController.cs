using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.AdvanceSalary;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/advance-salary")]
[Authorize]
public class AdvanceSalaryController(IAdvanceSalaryService service) : ControllerBase
{
    private const string ManageRoles =
        $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Accountant}";

    [HttpGet("my")]
    public async Task<IActionResult> GetMy([FromQuery] AdvanceSalaryFilterDto filter, CancellationToken ct = default)
        => Ok(ApiResponse<AdvanceSalaryMyListResponseDto>.Ok(await service.GetMyListAsync(filter, ct), "My advance salary requests retrieved"));

    [HttpPost("my")]
    public async Task<IActionResult> CreateMy(CreateMyAdvanceSalaryDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<AdvanceSalaryResponseDto>.Ok(await service.CreateMyAsync(dto, ct), "Advance salary request submitted"));

    [HttpGet]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> GetManage([FromQuery] AdvanceSalaryManageFilterDto filter, CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(filter.Export))
        {
            var file = await service.ExportAsync(filter, ct);
            return File(file.Content, file.ContentType, file.FileName);
        }
        return Ok(ApiResponse<AdvanceSalaryListResponseDto>.Ok(await service.GetManageListAsync(filter, ct), "Advance salary requests retrieved"));
    }

    [HttpPost]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Create(CreateAdvanceSalaryDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<AdvanceSalaryResponseDto>.Ok(await service.CreateForEmployeeAsync(dto, ct), "Advance salary request created"));

    [HttpPut("{id:guid}/approve")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct = default)
        => Ok(ApiResponse<AdvanceSalaryResponseDto>.Ok(await service.ApproveAsync(id, ct), "Advance salary approved"));

    [HttpPut("{id:guid}/reject")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Reject(Guid id, ReviewAdvanceSalaryDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<AdvanceSalaryResponseDto>.Ok(await service.RejectAsync(id, dto, ct), "Advance salary rejected"));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await service.DeleteAsync(id, ct);
        return Ok(ApiResponse.Ok("Advance salary request deleted"));
    }

    [HttpGet("export")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Export([FromQuery] AdvanceSalaryManageFilterDto filter, CancellationToken ct = default)
    {
        filter.Export ??= "csv";
        var file = await service.ExportAsync(filter, ct);
        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("lookup/employees")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Employees([FromQuery] string role, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<HrEmployeeLookupDto>>.Ok(await service.GetEmployeeLookupAsync(role, ct), "Employees retrieved"));
}
