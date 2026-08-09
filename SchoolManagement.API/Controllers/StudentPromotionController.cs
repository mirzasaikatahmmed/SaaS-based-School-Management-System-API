using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.Academic;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/academic/student-promotions")]
[Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
public class StudentPromotionController(IStudentPromotionService service) : ControllerBase
{
    [HttpGet("students")]
    public async Task<IActionResult> GetStudents([FromQuery] PromotionFilterDto filter, CancellationToken ct = default)
        => Ok(ApiResponse<PromotionStudentListResponseDto>.Ok(await service.GetStudentsAsync(filter, ct), "Students retrieved"));

    [HttpPost("process")]
    public async Task<IActionResult> Process(ProcessPromotionDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<ProcessPromotionResultDto>.Ok(await service.ProcessAsync(dto, ct), "Promotion processed"));
}
