using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.API.Filters;
using SchoolManagement.BLL.DTOs.Settings;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/settings/school")]
[Authorize]
public class SchoolSettingsExtrasController(
    ISchoolSettingsExtrasService extras,
    IEmailSettingsAppService email,
    ISmsSettingsAppService sms) : ControllerBase
{
    [HttpGet("{tenantSlug}/attendance-type")]
    [AuthorizePermission("Settings.SchoolSettings", AppConstants.PermissionActions.View)]
    public async Task<IActionResult> GetAttendanceType(string tenantSlug, CancellationToken ct = default)
        => Ok(ApiResponse<AttendanceTypeDto>.Ok(await extras.GetAttendanceTypeAsync(tenantSlug, ct), "Attendance type retrieved"));

    [HttpPatch("{tenantSlug}/attendance-type")]
    [AuthorizePermission("Settings.SchoolSettings", AppConstants.PermissionActions.Edit)]
    public async Task<IActionResult> UpdateAttendanceType(string tenantSlug, UpdateAttendanceTypeDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<AttendanceTypeDto>.Ok(await extras.UpdateAttendanceTypeAsync(tenantSlug, dto, ct), "Attendance type updated"));

    [HttpGet("{tenantSlug}/accounting-links")]
    [AuthorizePermission("Settings.SchoolSettings", AppConstants.PermissionActions.View)]
    public async Task<IActionResult> GetAccountingLinks(string tenantSlug, CancellationToken ct = default)
        => Ok(ApiResponse<AccountingLinksDto>.Ok(await extras.GetAccountingLinksAsync(tenantSlug, ct), "Accounting links retrieved"));

    [HttpPatch("{tenantSlug}/accounting-links")]
    [AuthorizePermission("Settings.SchoolSettings", AppConstants.PermissionActions.Edit)]
    public async Task<IActionResult> UpdateAccountingLinks(string tenantSlug, AccountingLinksDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<AccountingLinksDto>.Ok(await extras.UpdateAccountingLinksAsync(tenantSlug, dto, ct), "Accounting links updated"));

    [HttpGet("{tenantSlug}/email-config")]
    [AuthorizePermission("Settings.EmailSettings", AppConstants.PermissionActions.View)]
    public async Task<IActionResult> GetEmailConfig(string tenantSlug, CancellationToken ct = default)
        => Ok(ApiResponse<EmailConfigDto>.Ok(await email.GetConfigAsync(ct), "Email config retrieved"));

    [HttpPatch("{tenantSlug}/email-config")]
    [AuthorizePermission("Settings.EmailSettings", AppConstants.PermissionActions.Edit)]
    public async Task<IActionResult> UpdateEmailConfig(string tenantSlug, UpdateEmailConfigDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<EmailConfigDto>.Ok(await email.UpdateConfigAsync(dto, ct), "Email config updated"));

    [HttpPost("{tenantSlug}/email-config/test")]
    [AuthorizePermission("Settings.EmailSettings", AppConstants.PermissionActions.Edit)]
    public async Task<IActionResult> TestEmail(string tenantSlug, TestEmailDto dto, CancellationToken ct = default)
    {
        await email.TestAsync(dto, ct);
        return Ok(ApiResponse.Ok("Test email sent"));
    }

    [HttpGet("{tenantSlug}/email-triggers")]
    [AuthorizePermission("Settings.EmailSettings", AppConstants.PermissionActions.View)]
    public async Task<IActionResult> GetEmailTriggers(string tenantSlug, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<EmailTriggerDto>>.Ok(await email.GetTriggersAsync(ct), "Email triggers retrieved"));

    [HttpPatch("{tenantSlug}/email-triggers/{eventKey}")]
    [AuthorizePermission("Settings.EmailSettings", AppConstants.PermissionActions.Edit)]
    public async Task<IActionResult> UpdateEmailTrigger(string tenantSlug, string eventKey, UpdateEmailTriggerDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<EmailTriggerDto>.Ok(await email.UpdateTriggerAsync(eventKey, dto, ct), "Email trigger updated"));

    [HttpGet("{tenantSlug}/sms-config")]
    [AuthorizePermission("Settings.SmsSettings", AppConstants.PermissionActions.View)]
    public async Task<IActionResult> GetSmsConfig(string tenantSlug, CancellationToken ct = default)
        => Ok(ApiResponse<SmsConfigDto>.Ok(await sms.GetConfigAsync(ct), "SMS config retrieved"));

    [HttpPatch("{tenantSlug}/sms-config")]
    [AuthorizePermission("Settings.SmsSettings", AppConstants.PermissionActions.Edit)]
    public async Task<IActionResult> UpdateSmsConfig(string tenantSlug, UpdateSmsConfigDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<SmsConfigDto>.Ok(await sms.UpdateConfigAsync(dto, ct), "SMS config updated"));

    [HttpPatch("{tenantSlug}/sms-config/{gateway}")]
    [AuthorizePermission("Settings.SmsSettings", AppConstants.PermissionActions.Edit)]
    public async Task<IActionResult> UpdateSmsGateway(string tenantSlug, string gateway, [FromBody] Dictionary<string, string?> credentials, CancellationToken ct = default)
        => Ok(ApiResponse<SmsConfigDto>.Ok(await sms.UpdateGatewayCredentialsAsync(gateway, credentials, ct), "SMS gateway credentials updated"));

    [HttpPost("{tenantSlug}/sms-config/test")]
    [AuthorizePermission("Settings.SmsSettings", AppConstants.PermissionActions.Edit)]
    public async Task<IActionResult> TestSms(string tenantSlug, TestSmsDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<SmsTestResultDto>.Ok(await sms.TestSendAsync(dto, ct), "SMS test completed"));

    [HttpGet("{tenantSlug}/sms-config/balance")]
    [AuthorizePermission("Settings.SmsSettings", AppConstants.PermissionActions.View)]
    public async Task<IActionResult> SmsBalance(string tenantSlug, CancellationToken ct = default)
        => Ok(ApiResponse<SmsBalanceDto>.Ok(await sms.GetBalanceAsync(ct), "SMS balance retrieved"));

    [HttpGet("{tenantSlug}/sms-triggers")]
    [AuthorizePermission("Settings.SmsSettings", AppConstants.PermissionActions.View)]
    public async Task<IActionResult> GetSmsTriggers(string tenantSlug, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<SmsTriggerDto>>.Ok(await sms.GetTriggersAsync(ct), "SMS triggers retrieved"));

    [HttpPatch("{tenantSlug}/sms-triggers/{eventKey}")]
    [AuthorizePermission("Settings.SmsSettings", AppConstants.PermissionActions.Edit)]
    public async Task<IActionResult> UpdateSmsTrigger(string tenantSlug, string eventKey, UpdateSmsTriggerDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<SmsTriggerDto>.Ok(await sms.UpdateTriggerAsync(eventKey, dto, ct), "SMS trigger updated"));
}
