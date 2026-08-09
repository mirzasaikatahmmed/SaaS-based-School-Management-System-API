using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.ExamMaster;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/exam/exams")]
[Authorize]
public class ExamController(IExamService service) : ControllerBase
{
    private const string ManageRoles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}";
    private const string ReadRoles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Teacher},{AppConstants.Roles.Student}";

    [HttpGet]
    [Authorize(Roles = ReadRoles)]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<ExamListItemDto>>.Ok(await service.GetAllAsync(ct), "Exams retrieved"));

    [HttpGet("lookup")]
    [Authorize(Roles = ReadRoles)]
    public async Task<IActionResult> Lookup(CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<ExamLookupDto>>.Ok(await service.GetLookupAsync(ct), "Exams lookup retrieved"));

    [HttpGet("{id:guid}")]
    [Authorize(Roles = ReadRoles)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
        => Ok(ApiResponse<ExamResponseDto>.Ok(await service.GetByIdAsync(id, ct), "Exam retrieved"));

    [HttpPost]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Create(CreateExamDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<ExamResponseDto>.Ok(await service.CreateAsync(dto, ct), "Exam created"));

    [HttpPut("{id:guid}")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Update(Guid id, UpdateExamDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<ExamResponseDto>.Ok(await service.UpdateAsync(id, dto, ct), "Exam updated"));

    [HttpPut("{id:guid}/publish")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> TogglePublish(Guid id, CancellationToken ct = default)
        => Ok(ApiResponse<ExamResponseDto>.Ok(await service.TogglePublishAsync(id, ct), "Exam publish status updated"));

    [HttpPut("{id:guid}/publish-result")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> TogglePublishResult(Guid id, CancellationToken ct = default)
        => Ok(ApiResponse<ExamResponseDto>.Ok(await service.TogglePublishResultAsync(id, ct), "Exam result publish status updated"));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await service.DeleteAsync(id, ct);
        return Ok(ApiResponse.Ok("Exam deleted"));
    }
}
