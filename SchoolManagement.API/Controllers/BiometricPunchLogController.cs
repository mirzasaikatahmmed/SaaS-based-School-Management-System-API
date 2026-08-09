using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.Biometric;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/biometric/punches")]
[Authorize]
public class BiometricPunchLogController(IBiometricPunchService service) : ControllerBase
{
    private const string ManageRoles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Accountant}";
    private const string ViewRoles = $"{ManageRoles},{AppConstants.Roles.Teacher}";

    [HttpGet]
    [Authorize(Roles = ViewRoles)]
    public async Task<IActionResult> GetAll([FromQuery] PunchLogFilterDto filter, CancellationToken ct = default)
        => Ok(ApiResponse<PunchLogListResponseDto>.Ok(await service.GetPunchesAsync(filter, ct), "Punch logs retrieved"));

    [HttpPost("manual")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> RecordManual(ManualPunchDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<PunchLogItemDto>.Ok(await service.RecordManualPunchAsync(dto, ct), "Manual punch recorded"));
}
