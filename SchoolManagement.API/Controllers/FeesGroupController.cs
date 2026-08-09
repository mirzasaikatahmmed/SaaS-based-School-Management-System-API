using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.StudentAccounting;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/student-accounting/fees-groups")]
[Authorize]
public class FeesGroupController(IFeesGroupService service) : ControllerBase
{
    private const string ManageRoles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Accountant}";

    [HttpGet]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<FeesGroupResponseDto>>.Ok(await service.GetAllAsync(isActive, ct), "Fees groups retrieved"));

    [HttpGet("lookup")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Lookup(CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<FeesGroupLookupDto>>.Ok(await service.GetLookupAsync(ct), "Fees groups retrieved"));

    [HttpGet("{id:guid}")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
        => Ok(ApiResponse<FeesGroupResponseDto>.Ok(await service.GetByIdAsync(id, ct), "Fees group retrieved"));

    [HttpPost]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Create(CreateFeesGroupDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<FeesGroupResponseDto>.Ok(await service.CreateAsync(dto, ct), "Fees group created"));

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Update(Guid id, UpdateFeesGroupDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<FeesGroupResponseDto>.Ok(await service.UpdateAsync(id, dto, ct), "Fees group updated"));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await service.DeleteAsync(id, ct);
        return Ok(ApiResponse.Ok("Fees group deleted"));
    }
}
