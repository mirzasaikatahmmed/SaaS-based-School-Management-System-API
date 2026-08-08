using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.Student;
using SchoolManagement.BLL.DTOs.StudentList;
using StudentListPageDto = SchoolManagement.BLL.DTOs.StudentList.StudentListResponseDto;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/student-list")]
[Authorize]
public class StudentListController : ControllerBase
{
    private readonly IStudentListService _service;

    public StudentListController(IStudentListService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Teacher}")]
    public async Task<ActionResult<ApiResponse<StudentListPageDto>>> GetList(
        [FromQuery] StudentListFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetListAsync(filter, cancellationToken);
        var message = filter.ClassId.HasValue
            ? "Students retrieved"
            : "Class is required for filtering.";
        return Ok(ApiResponse<StudentListPageDto>.Ok(result, message));
    }

    [HttpGet("me")]
    [Authorize(Roles = AppConstants.Roles.Student)]
    public async Task<ActionResult<ApiResponse<StudentDetailDto>>> GetMe(
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetMeAsync(cancellationToken);
        return Ok(ApiResponse<StudentDetailDto>.Ok(result, "Student profile retrieved"));
    }

    [HttpGet("export")]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
    public async Task<IActionResult> Export(
        [FromQuery] StudentListFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var file = await _service.ExportAsync(filter, cancellationToken);
        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("login-deactivate")]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
    public async Task<ActionResult<ApiResponse<StudentListPageDto>>> GetLoginDeactivateList(
        [FromQuery] StudentListFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetLoginDeactivateListAsync(filter, cancellationToken);
        var message = filter.ClassId.HasValue
            ? "Login status list retrieved"
            : "Class is required for filtering.";
        return Ok(ApiResponse<StudentListPageDto>.Ok(result, message));
    }

    [HttpGet("deactivate-reasons")]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
    public async Task<ActionResult<ApiResponse<StudentListPageDto>>> GetDeactivateReasons(
        [FromQuery] StudentListFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetDeactivateReasonsAsync(filter, cancellationToken);
        return Ok(ApiResponse<StudentListPageDto>.Ok(result, "Deactivated students retrieved"));
    }

    [HttpPost("bulk-delete")]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
    public async Task<ActionResult<ApiResponse<BulkDeleteResultDto>>> BulkDelete(
        [FromBody] BulkDeleteDto dto,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.BulkDeleteAsync(dto, cancellationToken);
        return Ok(ApiResponse<BulkDeleteResultDto>.Ok(result, "Bulk delete completed"));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Teacher},{AppConstants.Roles.Parent},{AppConstants.Roles.Student}")]
    public async Task<ActionResult<ApiResponse<StudentDetailDto>>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<StudentDetailDto>.Ok(result, "Student retrieved"));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
    public async Task<ActionResult<ApiResponse<StudentDetailDto>>> Update(
        Guid id,
        [FromBody] UpdateAdmissionDto dto,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.UpdateAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<StudentDetailDto>.Ok(result, "Student updated"));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _service.SoftDeleteAsync(id, cancellationToken);
        return Ok(ApiResponse.Ok("Student deleted"));
    }

    [HttpPut("{id:guid}/toggle-login")]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
    public async Task<ActionResult<ApiResponse<object>>> ToggleLogin(
        Guid id,
        [FromBody] LoginDeactivateDto dto,
        CancellationToken cancellationToken = default)
    {
        await _service.ToggleLoginAsync(id, dto, cancellationToken);
        return Ok(ApiResponse.Ok(dto.IsLoginActive ? "Login activated" : "Login deactivated"));
    }

    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
    public async Task<ActionResult<ApiResponse<object>>> Deactivate(
        Guid id,
        [FromBody] DeactivateReasonDto dto,
        CancellationToken cancellationToken = default)
    {
        await _service.DeactivateAsync(id, dto, cancellationToken);
        return Ok(ApiResponse.Ok("Student deactivated"));
    }

    [HttpPut("{id:guid}/activate")]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
    public async Task<ActionResult<ApiResponse<object>>> Activate(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _service.ActivateAsync(id, cancellationToken);
        return Ok(ApiResponse.Ok("Student re-activated"));
    }
}
