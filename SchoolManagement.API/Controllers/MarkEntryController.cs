using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.ExamMaster;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/exam/mark-entries")]
[Authorize]
public class MarkEntryController(IMarkEntryService service) : ControllerBase
{
    private const string ManageRoles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Teacher}";
    private const string ReadRoles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Teacher},{AppConstants.Roles.Student}";

    [HttpGet]
    [Authorize(Roles = ReadRoles)]
    public async Task<IActionResult> GetList([FromQuery] MarkEntryFilterDto filter, CancellationToken ct = default)
        => Ok(ApiResponse<MarkEntryListResponseDto>.Ok(await service.GetListAsync(filter, ct), "Mark entries retrieved"));

    [HttpPost("save")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Save(SaveMarkEntriesDto dto, CancellationToken ct = default)
    {
        await service.SaveAsync(dto, ct);
        return Ok(ApiResponse.Ok("Mark entries saved"));
    }

    [HttpGet("export")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Export([FromQuery] MarkEntryFilterDto filter, CancellationToken ct = default)
    {
        filter.Export ??= "csv";
        var file = await service.ExportAsync(filter, ct);
        return File(file.Content, file.ContentType, file.FileName);
    }
}
