using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.StudentAccounting;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/student-accounting/fees-reminders")]
[Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Accountant}")]
public class FeesReminderController(IFeesReminderService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<FeesReminderResponseDto>>.Ok(await service.GetAllAsync(ct), "Fees reminders retrieved"));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
        => Ok(ApiResponse<FeesReminderResponseDto>.Ok(await service.GetByIdAsync(id, ct), "Fees reminder retrieved"));

    [HttpPost]
    public async Task<IActionResult> Create(CreateFeesReminderDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<FeesReminderResponseDto>.Ok(await service.CreateAsync(dto, ct), "Fees reminder created"));

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateFeesReminderDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<FeesReminderResponseDto>.Ok(await service.UpdateAsync(id, dto, ct), "Fees reminder updated"));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await service.DeleteAsync(id, ct);
        return Ok(ApiResponse.Ok("Fees reminder deleted"));
    }
}
