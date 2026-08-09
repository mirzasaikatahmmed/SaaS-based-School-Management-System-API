using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.AdvanceSalary;
using SchoolManagement.BLL.DTOs.Leave;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/leave")]
[Authorize]
public class LeaveController(ILeaveService service) : ControllerBase
{
    private const string ManageRoles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}";

    [HttpGet("my")]
    public async Task<IActionResult> GetMy([FromQuery] LeaveFilterDto filter, CancellationToken ct = default)
        => Ok(ApiResponse<LeaveListResponseDto>.Ok(await service.GetMyListAsync(filter, ct), "My leave requests retrieved"));

    [HttpPost("my")]
    public async Task<IActionResult> CreateMy(CreateLeaveRequestDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<LeaveListItemDto>.Ok(await service.CreateMyAsync(dto, ct), "Leave request submitted"));

    [HttpDelete("my/{id:guid}")]
    public async Task<IActionResult> CancelMy(Guid id, CancellationToken ct = default)
    {
        await service.CancelMyAsync(id, ct);
        return Ok(ApiResponse.Ok("Leave request cancelled"));
    }

    [HttpGet]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> GetManage([FromQuery] LeaveManageFilterDto filter, CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(filter.Export))
        {
            var file = await service.ExportAsync(filter, ct);
            return File(file.Content, file.ContentType, file.FileName);
        }
        return Ok(ApiResponse<LeaveListResponseDto>.Ok(await service.GetManageListAsync(filter, ct), "Leave requests retrieved"));
    }

    [HttpPost]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> AdminCreate(AdminCreateLeaveRequestDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<LeaveListItemDto>.Ok(await service.AdminCreateAsync(dto, ct), "Leave request created"));

    [HttpPut("{id:guid}/approve")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Approve(Guid id, ReviewLeaveDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<LeaveListItemDto>.Ok(await service.ApproveAsync(id, dto, ct), "Leave approved"));

    [HttpPut("{id:guid}/reject")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Reject(Guid id, ReviewLeaveDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<LeaveListItemDto>.Ok(await service.RejectAsync(id, dto, ct), "Leave rejected"));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await service.DeleteAsync(id, ct);
        return Ok(ApiResponse.Ok("Leave request deleted"));
    }

    [HttpGet("export")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Export([FromQuery] LeaveManageFilterDto filter, CancellationToken ct = default)
    {
        filter.Export ??= "csv";
        var file = await service.ExportAsync(filter, ct);
        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpPost("{id:guid}/attachment")]
    [Authorize(Roles = ManageRoles)]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> Attachment(Guid id, IFormFile file, CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse.Fail("File is required."));
        await using var stream = file.OpenReadStream();
        return Ok(ApiResponse<LeaveListItemDto>.Ok(
            await service.UploadAttachmentAsync(id, stream, file.FileName, file.ContentType, ct),
            "Leave attachment uploaded"));
    }

    [HttpGet("lookup/leave-types")]
    public async Task<IActionResult> LeaveTypes(CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<LeaveCategoryLookupDto>>.Ok(await service.GetMyLeaveTypesAsync(ct), "Leave types retrieved"));

    [HttpGet("lookup/employees")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Employees([FromQuery] string role, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<HrEmployeeLookupDto>>.Ok(await service.GetEmployeeLookupAsync(role, ct), "Employees retrieved"));
}
