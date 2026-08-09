using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.ExamMaster;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/exam/schedules")]
[Authorize]
public class ExamScheduleController(IExamScheduleService service) : ControllerBase
{
    private const string ManageRoles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}";
    private const string ReadRoles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Teacher},{AppConstants.Roles.Student}";

    [HttpGet]
    [Authorize(Roles = ReadRoles)]
    public async Task<IActionResult> GetFiltered([FromQuery] ExamScheduleFilterDto filter, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<ExamScheduleResponseDto>>.Ok(await service.GetFilteredAsync(filter, ct), "Exam schedules retrieved"));

    [HttpGet("{id:guid}")]
    [Authorize(Roles = ReadRoles)]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken ct = default)
        => Ok(ApiResponse<ExamScheduleDetailDto>.Ok(await service.GetDetailAsync(id, ct), "Exam schedule detail retrieved"));

    [HttpPost]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Create(CreateExamScheduleDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<ExamScheduleDetailDto>.Ok(await service.CreateAsync(dto, ct), "Exam schedule created"));

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Update(Guid id, CreateExamScheduleDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<ExamScheduleDetailDto>.Ok(await service.UpdateAsync(id, dto, ct), "Exam schedule updated"));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await service.DeleteAsync(id, ct);
        return Ok(ApiResponse.Ok("Exam schedule deleted"));
    }
}
