using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.ExamMaster;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/exam/halls")]
[Authorize]
public class ExamHallController(IExamHallService service) : ControllerBase
{
    private const string ManageRoles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}";
    private const string ReadRoles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Teacher}";

    [HttpGet]
    [Authorize(Roles = ReadRoles)]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<ExamHallResponseDto>>.Ok(await service.GetAllAsync(ct), "Exam halls retrieved"));

    [HttpGet("lookup")]
    [Authorize(Roles = ReadRoles)]
    public async Task<IActionResult> Lookup(CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<ExamHallLookupDto>>.Ok(await service.GetLookupAsync(ct), "Exam halls lookup retrieved"));

    [HttpPost]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Create(CreateExamHallDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<ExamHallResponseDto>.Ok(await service.CreateAsync(dto, ct), "Exam hall created"));

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Update(Guid id, UpdateExamHallDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<ExamHallResponseDto>.Ok(await service.UpdateAsync(id, dto, ct), "Exam hall updated"));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await service.DeleteAsync(id, ct);
        return Ok(ApiResponse.Ok("Exam hall deleted"));
    }
}
