using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.Marks;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/marks/positions")]
[Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Teacher}")]
public class ExamPositionController(IExamPositionService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ExamPositionFilterDto filter, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<ExamPositionItemDto>>.Ok(await service.GetAsync(filter, ct), "Exam positions retrieved"));

    [HttpPost("generate")]
    public async Task<IActionResult> Generate(ExamPositionFilterDto filter, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<ExamPositionItemDto>>.Ok(await service.GenerateAsync(filter, ct), "Exam positions generated"));

    [HttpPatch("save")]
    public async Task<IActionResult> Save(SaveExamPositionDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<ExamPositionItemDto>>.Ok(await service.SaveAsync(dto, ct), "Exam positions saved"));
}
