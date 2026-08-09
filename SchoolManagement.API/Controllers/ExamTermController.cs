using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.ExamMaster;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/exam/terms")]
[Authorize]
public class ExamTermController(IExamTermService service) : ControllerBase
{
    private const string ManageRoles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}";
    private const string ReadRoles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Teacher}";

    [HttpGet]
    [Authorize(Roles = ReadRoles)]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<ExamTermResponseDto>>.Ok(await service.GetAllAsync(ct), "Exam terms retrieved"));

    [HttpPost]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Create(CreateExamTermDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<ExamTermResponseDto>.Ok(await service.CreateAsync(dto, ct), "Exam term created"));

    [HttpPut("{id:guid}")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Update(Guid id, UpdateExamTermDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<ExamTermResponseDto>.Ok(await service.UpdateAsync(id, dto, ct), "Exam term updated"));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await service.DeleteAsync(id, ct);
        return Ok(ApiResponse.Ok("Exam term deleted"));
    }
}
