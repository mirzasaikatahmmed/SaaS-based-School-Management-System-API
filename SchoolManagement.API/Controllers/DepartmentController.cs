using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.Employee;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;
namespace SchoolManagement.API.Controllers;
[ApiController,Route("api/departments"),Authorize]
public class DepartmentController(IDepartmentService service):ControllerBase
{
 [HttpGet,Authorize(Roles=$"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Teacher}")] public async Task<IActionResult> GetAll(CancellationToken ct=default)=>Ok(ApiResponse<IReadOnlyList<DepartmentResponseDto>>.Ok(await service.GetAllAsync(ct),"Departments retrieved"));
 [HttpPost,Authorize(Roles=$"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")] public async Task<IActionResult> Create(CreateDepartmentDto dto,CancellationToken ct=default)=>Ok(ApiResponse<DepartmentResponseDto>.Ok(await service.CreateAsync(dto,ct),"Department created"));
 [HttpPatch("{id:guid}"),Authorize(Roles=$"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")] public async Task<IActionResult> Update(Guid id,UpdateDepartmentDto dto,CancellationToken ct=default)=>Ok(ApiResponse<DepartmentResponseDto>.Ok(await service.UpdateAsync(id,dto,ct),"Department updated"));
 [HttpDelete("{id:guid}"),Authorize(Roles=$"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")] public async Task<IActionResult> Delete(Guid id,CancellationToken ct=default){await service.DeleteAsync(id,ct);return Ok(ApiResponse.Ok("Department deleted"));}
}
