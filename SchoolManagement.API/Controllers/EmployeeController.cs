using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.Employee;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;
namespace SchoolManagement.API.Controllers;
[ApiController,Route("api/employees"),Authorize]
public class EmployeeController(IEmployeeService service,IEmployeeImportService imports):ControllerBase
{
 [HttpGet,Authorize(Roles=$"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")] public async Task<IActionResult> GetList([FromQuery]EmployeeListFilterDto filter,CancellationToken ct=default)=>Ok(ApiResponse<EmployeeListResponseDto>.Ok(await service.GetListAsync(filter,ct),"Employees retrieved"));
 [HttpGet("me"),Authorize(Roles=AppConstants.Roles.Teacher)] public async Task<IActionResult> Me(CancellationToken ct=default)=>Ok(ApiResponse<EmployeeDetailDto>.Ok(await service.GetMeAsync(ct),"Employee profile retrieved"));
 [HttpGet("export"),Authorize(Roles=$"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")] public async Task<IActionResult> Export([FromQuery]EmployeeListFilterDto filter,CancellationToken ct=default){var x=await service.ExportAsync(filter,ct);return File(x.Content,x.ContentType,x.FileName);}
 [HttpGet("import/sample-csv"),Authorize(Roles=$"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")] public IActionResult Sample()=>File(imports.GetSampleCsv(),"text/csv","employee_import_sample.csv");
 [HttpPost("import"),Authorize(Roles=$"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}"),RequestSizeLimit(6*1024*1024)] public async Task<IActionResult> Import(IFormFile csvFile,CancellationToken ct=default){if(csvFile is null||csvFile.Length==0)return BadRequest(ApiResponse.Fail("CsvFile is required."));await using var s=csvFile.OpenReadStream();return Ok(ApiResponse<EmployeeImportBatchDto>.Ok(await imports.ImportAsync(s,csvFile.FileName,ct),"Import completed"));}
 [HttpGet("import/batches"),Authorize(Roles=$"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")] public async Task<IActionResult> Batches(CancellationToken ct=default)=>Ok(ApiResponse<IReadOnlyList<EmployeeImportBatchDto>>.Ok(await imports.GetBatchesAsync(ct),"Import batches retrieved"));
 [HttpGet("import/batches/{id:guid}"),Authorize(Roles=$"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")] public async Task<IActionResult> Batch(Guid id,CancellationToken ct=default)=>Ok(ApiResponse<EmployeeImportBatchDto>.Ok(await imports.GetBatchByIdAsync(id,ct),"Import batch retrieved"));
 [HttpGet("lookup/roles"),Authorize(Roles=$"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")] public async Task<IActionResult> Roles(CancellationToken ct=default)=>Ok(ApiResponse<IReadOnlyList<string>>.Ok(await service.GetRolesLookupAsync(ct),"Roles retrieved"));
 [HttpGet("lookup/departments"),Authorize(Roles=$"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")] public async Task<IActionResult> Departments(CancellationToken ct=default)=>Ok(ApiResponse<IReadOnlyList<DepartmentResponseDto>>.Ok(await service.GetDepartmentsLookupAsync(ct),"Departments retrieved"));
 [HttpGet("lookup/designations"),Authorize(Roles=$"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")] public async Task<IActionResult> Designations(CancellationToken ct=default)=>Ok(ApiResponse<IReadOnlyList<DesignationResponseDto>>.Ok(await service.GetDesignationsLookupAsync(ct),"Designations retrieved"));
 [HttpPost,Authorize(Roles=$"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")] public async Task<IActionResult> Create(AddEmployeeDto dto,CancellationToken ct=default){var x=await service.CreateAsync(dto,ct);return CreatedAtAction(nameof(Get),new{id=x.Id},ApiResponse<EmployeeDetailDto>.Ok(x,"Employee created"));}
 [HttpPost("{id:guid}/photo"),Authorize(Roles=$"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}"),RequestSizeLimit(3*1024*1024)] public async Task<IActionResult> Photo(Guid id,IFormFile file,CancellationToken ct=default){if(file is null||file.Length==0)return BadRequest(ApiResponse.Fail("File is required."));await using var s=file.OpenReadStream();return Ok(ApiResponse<EmployeeDetailDto>.Ok(await service.UploadPhotoAsync(id,s,file.FileName,file.ContentType,ct),"Employee photo uploaded"));}
 [HttpGet("{id:guid}"),Authorize(Roles=$"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")] public async Task<IActionResult> Get(Guid id,CancellationToken ct=default)=>Ok(ApiResponse<EmployeeDetailDto>.Ok(await service.GetByIdAsync(id,ct),"Employee retrieved"));
 [HttpPatch("{id:guid}"),Authorize(Roles=$"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")] public async Task<IActionResult> Update(Guid id,UpdateEmployeeDto dto,CancellationToken ct=default)=>Ok(ApiResponse<EmployeeDetailDto>.Ok(await service.UpdateAsync(id,dto,ct),"Employee updated"));
 [HttpDelete("{id:guid}"),Authorize(Roles=$"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")] public async Task<IActionResult> Delete(Guid id,CancellationToken ct=default){await service.SoftDeleteAsync(id,ct);return Ok(ApiResponse.Ok("Employee deleted"));}
}
