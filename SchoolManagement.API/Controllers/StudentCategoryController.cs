using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.StudentCategory;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/student-categories")]
[Authorize]
public class StudentCategoryController : ControllerBase
{
    private readonly IStudentCategoryService _service;

    public StudentCategoryController(IStudentCategoryService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Teacher}")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StudentCategoryResponseDto>>>> GetAll(
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<StudentCategoryResponseDto>>.Ok(result, "Categories retrieved"));
    }

    [HttpPost]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
    public async Task<ActionResult<ApiResponse<StudentCategoryResponseDto>>> Create(
        [FromBody] CreateStudentCategoryDto dto,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetAll), null,
            ApiResponse<StudentCategoryResponseDto>.Ok(result, "Category created"));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
    public async Task<ActionResult<ApiResponse<StudentCategoryResponseDto>>> Update(
        Guid id,
        [FromBody] UpdateStudentCategoryDto dto,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.UpdateAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<StudentCategoryResponseDto>.Ok(result, "Category updated"));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse.Ok("Category deleted"));
    }
}
