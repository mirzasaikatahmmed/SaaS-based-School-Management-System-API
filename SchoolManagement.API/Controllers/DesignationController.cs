using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.Employee;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;
namespace SchoolManagement.API.Controllers;
[ApiController,Route("api/designations"),Authorize]
public class DesignationController(IDesignationService service):ControllerBase
{
 [HttpGet,Authorize(Roles=$"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Teacher}")] public async Task<IActionResult> GetAll(CancellationToken ct=default)=>Ok(ApiResponse<IReadOnlyList<DesignationResponseDto>>.Ok(await service.GetAllAsync(ct),"Designations retrieved"));
 [HttpPost,Authorize(Roles=$"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")] public async Task<IActionResult> Create(CreateDesignationDto dto,CancellationToken ct=default)=>Ok(ApiResponse<DesignationResponseDto>.Ok(await service.CreateAsync(dto,ct),"Designation created"));
 [HttpPut("{id:guid}"),Authorize(Roles=$"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")] public async Task<IActionResult> Update(Guid id,UpdateDesignationDto dto,CancellationToken ct=default)=>Ok(ApiResponse<DesignationResponseDto>.Ok(await service.UpdateAsync(id,dto,ct),"Designation updated"));
 [HttpDelete("{id:guid}"),Authorize(Roles=$"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")] public async Task<IActionResult> Delete(Guid id,CancellationToken ct=default){await service.DeleteAsync(id,ct);return Ok(ApiResponse.Ok("Designation deleted"));}
}
