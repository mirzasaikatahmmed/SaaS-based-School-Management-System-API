using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.API.Filters;
using SchoolManagement.BLL.DTOs.Settings;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize]
public class RoleController(IRoleService service) : ControllerBase
{
    [HttpGet]
    [AuthorizePermission("Settings.RolesPermissions", AppConstants.PermissionActions.View)]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<RoleResponseDto>>.Ok(await service.GetAllAsync(ct), "Roles retrieved"));

    [HttpPost]
    [AuthorizePermission("Settings.RolesPermissions", AppConstants.PermissionActions.Add)]
    public async Task<IActionResult> Create(CreateRoleDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<RoleResponseDto>.Ok(await service.CreateAsync(dto, ct), "Role created"));

    [HttpPatch("{id:guid}")]
    [AuthorizePermission("Settings.RolesPermissions", AppConstants.PermissionActions.Edit)]
    public async Task<IActionResult> Update(Guid id, UpdateRoleDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<RoleResponseDto>.Ok(await service.UpdateAsync(id, dto, ct), "Role updated"));

    [HttpDelete("{id:guid}")]
    [AuthorizePermission("Settings.RolesPermissions", AppConstants.PermissionActions.Delete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await service.DeleteAsync(id, ct);
        return Ok(ApiResponse.Ok("Role deleted"));
    }

    [HttpGet("{id:guid}/permissions")]
    [AuthorizePermission("Settings.RolesPermissions", AppConstants.PermissionActions.View)]
    public async Task<IActionResult> GetPermissions(Guid id, CancellationToken ct = default)
        => Ok(ApiResponse<RolePermissionMatrixDto>.Ok(await service.GetPermissionsAsync(id, ct), "Permissions retrieved"));

    [HttpPatch("{id:guid}/permissions")]
    [AuthorizePermission("Settings.RolesPermissions", AppConstants.PermissionActions.Edit)]
    public async Task<IActionResult> UpdatePermissions(Guid id, UpdateRolePermissionsDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<RolePermissionMatrixDto>.Ok(await service.UpdatePermissionsAsync(id, dto, ct), "Permissions updated"));
}
