using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.Student;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/admission")]
[Authorize]
public class AdmissionController : ControllerBase
{
    private readonly IStudentService _studentService;

    public AdmissionController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    [HttpGet]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Teacher},{AppConstants.Roles.Parent},{AppConstants.Roles.Student}")]
    public async Task<ActionResult<ApiResponse<StudentListResponseDto>>> GetStudents(
        [FromQuery] string? search,
        [FromQuery] int? academicYear,
        [FromQuery] Guid? classId,
        [FromQuery] Guid? sectionId,
        [FromQuery] Guid? categoryId,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _studentService.GetStudentsAsync(new StudentSearchFilter
        {
            Search = search,
            AcademicYear = academicYear,
            ClassId = classId,
            SectionId = sectionId,
            CategoryId = categoryId,
            IsActive = isActive,
            Page = page,
            PageSize = pageSize
        }, cancellationToken);

        return Ok(ApiResponse<StudentListResponseDto>.Ok(result, "Students retrieved"));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Teacher},{AppConstants.Roles.Parent},{AppConstants.Roles.Student}")]
    public async Task<ActionResult<ApiResponse<StudentResponseDto>>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _studentService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<StudentResponseDto>.Ok(result, "Student retrieved"));
    }

    [HttpPost]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
    public async Task<ActionResult<ApiResponse<StudentResponseDto>>> Create(
        [FromBody] CreateAdmissionDto dto,
        CancellationToken cancellationToken = default)
    {
        var result = await _studentService.CreateAdmissionAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<StudentResponseDto>.Ok(result, "Admission created successfully"));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
    public async Task<ActionResult<ApiResponse<StudentResponseDto>>> Update(
        Guid id,
        [FromBody] UpdateAdmissionDto dto,
        CancellationToken cancellationToken = default)
    {
        var result = await _studentService.UpdateAdmissionAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<StudentResponseDto>.Ok(result, "Admission updated"));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _studentService.SoftDeleteAsync(id, cancellationToken);
        return Ok(ApiResponse.Ok("Student deactivated"));
    }

    [HttpPost("{id:guid}/profile-picture")]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
    [RequestSizeLimit(3 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<StudentResponseDto>>> UploadProfilePicture(
        Guid id,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse.Fail("File is required."));

        await using var stream = file.OpenReadStream();
        var result = await _studentService.UploadProfilePictureAsync(
            id, stream, file.FileName, file.ContentType, cancellationToken);
        return Ok(ApiResponse<StudentResponseDto>.Ok(result, "Profile picture uploaded"));
    }

    [HttpPost("{id:guid}/guardian-picture")]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
    [RequestSizeLimit(3 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<StudentResponseDto>>> UploadGuardianPicture(
        Guid id,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse.Fail("File is required."));

        await using var stream = file.OpenReadStream();
        var result = await _studentService.UploadGuardianPictureAsync(
            id, stream, file.FileName, file.ContentType, cancellationToken);
        return Ok(ApiResponse<StudentResponseDto>.Ok(result, "Guardian picture uploaded"));
    }
}
