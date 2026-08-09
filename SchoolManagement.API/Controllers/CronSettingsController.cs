using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.API.Filters;
using SchoolManagement.BLL.DTOs.Settings;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/settings/cron")]
[Authorize]
public class CronSettingsController(ICronSettingsService service) : ControllerBase
{
    [HttpGet]
    [AuthorizePermission("Settings.Cron", AppConstants.PermissionActions.View)]
    public async Task<IActionResult> Get(CancellationToken ct = default)
        => Ok(ApiResponse<CronSettingsResponseDto>.Ok(await service.GetAsync(ct), "Cron settings retrieved"));

    [HttpPost("regenerate-key")]
    [AuthorizePermission("Settings.Cron", AppConstants.PermissionActions.Edit)]
    public async Task<IActionResult> Regenerate(CancellationToken ct = default)
        => Ok(ApiResponse<CronSettingsResponseDto>.Ok(await service.RegenerateKeyAsync(ct), "Cron secret regenerated"));
}

[ApiController]
[Route("cron_api")]
[AllowAnonymous]
public class CronApiController(ICronSettingsService service) : ControllerBase
{
    [HttpGet("send_smsemail_command/{secretKey}")]
    public async Task<IActionResult> SendSmsEmail(string secretKey, CancellationToken ct = default)
        => Ok(await service.RunSendSmsEmailAsync(secretKey, ct));

    [HttpGet("homework_command/{secretKey}")]
    public async Task<IActionResult> Homework(string secretKey, CancellationToken ct = default)
        => Ok(await service.RunHomeworkAsync(secretKey, ct));

    [HttpGet("fees_reminder_command/{secretKey}")]
    public async Task<IActionResult> FeesReminder(string secretKey, CancellationToken ct = default)
        => Ok(await service.RunFeesReminderAsync(secretKey, ct));
}
