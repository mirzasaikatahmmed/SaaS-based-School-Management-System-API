using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.Employee;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;
namespace SchoolManagement.API.Controllers;
[ApiController,Route("api/employee-login-deactivate"),Authorize(Roles=$"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
public class EmployeeLoginDeactivateController(IEmployeeService service):ControllerBase
{
 [HttpGet] public async Task<IActionResult> List([FromQuery]EmployeeLoginDeactivateFilterDto filter,CancellationToken ct=default)=>Ok(ApiResponse<EmployeeLoginDeactivateListResponseDto>.Ok(await service.GetLoginDeactivateListAsync(filter,ct),"Login-deactivated employees retrieved"));
 [HttpPut("{id:guid}/activate")] public async Task<IActionResult> Activate(Guid id,CancellationToken ct=default){await service.ActivateLoginAsync(id,ct);return Ok(ApiResponse.Ok("Employee login activated"));}
 [HttpPut("{id:guid}/deactivate")] public async Task<IActionResult> Deactivate(Guid id,CancellationToken ct=default){await service.DeactivateLoginAsync(id,ct);return Ok(ApiResponse.Ok("Employee login deactivated"));}
 [HttpPost("bulk-activate")] public async Task<IActionResult> Bulk(BulkEmployeeActivateDto dto,CancellationToken ct=default)=>Ok(ApiResponse<BulkEmployeeActivateResultDto>.Ok(await service.BulkActivateLoginAsync(dto,ct),"Bulk activate completed"));
}
