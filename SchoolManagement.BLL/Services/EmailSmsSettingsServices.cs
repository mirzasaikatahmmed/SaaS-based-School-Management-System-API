using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SchoolManagement.BLL.DTOs.Settings;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class EmailSettingsService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http,
    IDataProtectionProvider dataProtection,
    ILogger<EmailSettingsService> logger) : IEmailSettingsAppService
{
    private readonly IDataProtector _protector = dataProtection.CreateProtector("SchoolManagement.SmtpPassword");

    public async Task<EmailConfigDto> GetConfigAsync(CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        Manage();
        var s = await uow.EmailSettings.GetOrCreateAsync(cancellationToken);
        await uow.SaveTenantChangesAsync(cancellationToken);
        return MapConfig(s);
    }

    public async Task<EmailConfigDto> UpdateConfigAsync(UpdateEmailConfigDto dto, CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        Manage();
        if (!EmailProtocols.IsValid(dto.Protocol))
            throw new AppException("Protocol must be SMTP.", 400);
        if (!SmtpSecureModes.IsValid(dto.SmtpSecure))
            throw new AppException("SmtpSecure must be None, SSL, or TLS.", 400);

        var s = await uow.EmailSettings.GetOrCreateAsync(cancellationToken);
        s.IsEnabled = dto.IsEnabled;
        s.SystemEmail = dto.SystemEmail?.Trim();
        s.Protocol = dto.Protocol.Trim().ToUpperInvariant();
        s.SmtpHost = dto.SmtpHost?.Trim();
        s.SmtpPort = dto.SmtpPort;
        s.SmtpUsername = dto.SmtpUsername?.Trim();
        s.SmtpSecure = dto.SmtpSecure.Trim();
        s.SmtpAuth = dto.SmtpAuth;
        s.FromName = dto.FromName?.Trim();
        if (!string.IsNullOrWhiteSpace(dto.SmtpPassword))
            s.SmtpPassword = _protector.Protect(dto.SmtpPassword);

        await uow.EmailSettings.UpdateAsync(s, cancellationToken);
        await uow.SaveTenantChangesAsync(cancellationToken);
        return MapConfig(s);
    }

    public async Task TestAsync(TestEmailDto dto, CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        Manage();
        var to = dto.To.Trim();
        if (string.IsNullOrWhiteSpace(to))
            throw new AppException("Recipient email is required.", 400);

        var s = await uow.EmailSettings.GetOrCreateAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(s.SmtpHost) || string.IsNullOrWhiteSpace(s.SystemEmail))
            throw new AppException("SMTP host and system email must be configured first.", 400);

        var password = Decrypt(s.SmtpPassword);
        try
        {
            using var client = new SmtpClient(s.SmtpHost, s.SmtpPort)
            {
                EnableSsl = !s.SmtpSecure.Equals(SmtpSecureModes.None, StringComparison.OrdinalIgnoreCase),
                DeliveryMethod = SmtpDeliveryMethod.Network
            };
            if (s.SmtpAuth && !string.IsNullOrWhiteSpace(s.SmtpUsername))
                client.Credentials = new NetworkCredential(s.SmtpUsername, password ?? string.Empty);

            using var msg = new MailMessage(s.SystemEmail!, to)
            {
                Subject = "School Management — Test Email",
                Body = "<p>This is a test email from your school settings.</p>",
                IsBodyHtml = true
            };
            if (!string.IsNullOrWhiteSpace(s.FromName))
                msg.From = new MailAddress(s.SystemEmail!, s.FromName);

            await client.SendMailAsync(msg, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Test email failed");
            throw new AppException($"Failed to send test email: {ex.Message}", 400);
        }
    }

    public async Task<IReadOnlyList<EmailTriggerDto>> GetTriggersAsync(CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        Manage();
        var saved = (await uow.EmailSettings.GetTemplatesAsync(cancellationToken))
            .ToDictionary(t => t.EventKey, StringComparer.OrdinalIgnoreCase);

        return NotificationEventKeys.EmailDefaults.Select(d =>
        {
            saved.TryGetValue(d.Key, out var row);
            return new EmailTriggerDto
            {
                EventKey = d.Key,
                Name = d.Name,
                NotifyEnabled = row?.NotifyEnabled ?? true,
                Subject = row?.Subject ?? d.DefaultSubject,
                BodyHtml = row?.BodyHtml ?? d.DefaultBody
            };
        }).ToList();
    }

    public async Task<EmailTriggerDto> UpdateTriggerAsync(string eventKey, UpdateEmailTriggerDto dto, CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        Manage();
        var def = NotificationEventKeys.EmailDefaults.FirstOrDefault(x => x.Key.Equals(eventKey, StringComparison.OrdinalIgnoreCase));
        if (def.Key is null)
            throw new NotFoundException($"Unknown email event key '{eventKey}'.");

        await uow.EmailSettings.UpsertTemplateAsync(new EmailTemplate
        {
            EventKey = def.Key,
            Name = def.Name,
            Subject = dto.Subject.Trim(),
            BodyHtml = dto.BodyHtml,
            NotifyEnabled = dto.NotifyEnabled
        }, cancellationToken);
        await uow.SaveTenantChangesAsync(cancellationToken);

        return new EmailTriggerDto
        {
            EventKey = def.Key,
            Name = def.Name,
            NotifyEnabled = dto.NotifyEnabled,
            Subject = dto.Subject.Trim(),
            BodyHtml = dto.BodyHtml
        };
    }

    private EmailConfigDto MapConfig(EmailSettings s) => new()
    {
        IsEnabled = s.IsEnabled,
        SystemEmail = s.SystemEmail,
        Protocol = s.Protocol,
        SmtpHost = s.SmtpHost,
        SmtpPort = s.SmtpPort,
        SmtpUsername = s.SmtpUsername,
        HasPassword = !string.IsNullOrEmpty(s.SmtpPassword),
        SmtpSecure = s.SmtpSecure,
        SmtpAuth = s.SmtpAuth,
        FromName = s.FromName
    };

    private string? Decrypt(string? protectedValue)
    {
        if (string.IsNullOrEmpty(protectedValue)) return null;
        try { return _protector.Unprotect(protectedValue); }
        catch { return protectedValue; }
    }

    private async Task Ready(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(tenant.SchemaName))
            throw new AppException("X-Tenant-ID header is required.", 400);
        await provisioner.EnsureSettingsModuleAsync(tenant.SchemaName!, ct);
    }

    private HashSet<string> Roles() =>
        http.HttpContext?.User.FindAll("role").Concat(http.HttpContext.User.FindAll(ClaimTypes.Role))
            .Select(x => x.Value).ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];

    private void Manage()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin))
            throw new ForbiddenException("Only Super Admin or School Admin can manage email settings.");
    }
}

public class SmsSettingsService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http,
    ISmsSenderFactory smsFactory) : ISmsSettingsAppService
{
    public async Task<SmsConfigDto> GetConfigAsync(CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        Manage();
        var s = await uow.SmsSettings.GetOrCreateAsync(cancellationToken);
        await uow.SaveTenantChangesAsync(cancellationToken);
        return Map(s);
    }

    public async Task<SmsConfigDto> UpdateConfigAsync(UpdateSmsConfigDto dto, CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        Manage();
        if (!SmsGateways.IsValid(dto.ActivatedGateway))
            throw new AppException($"Unknown SMS gateway '{dto.ActivatedGateway}'.", 400);

        var s = await uow.SmsSettings.GetOrCreateAsync(cancellationToken);
        s.IsEnabled = dto.IsEnabled;
        s.ActivatedGateway = dto.ActivatedGateway.Trim().ToLowerInvariant();
        if (dto.Credentials is not null)
            s.CredentialsJson = JsonSerializer.Serialize(dto.Credentials);

        await uow.SmsSettings.UpdateAsync(s, cancellationToken);
        await uow.SaveTenantChangesAsync(cancellationToken);
        return Map(s);
    }

    public async Task<SmsConfigDto> UpdateGatewayCredentialsAsync(
        string gateway,
        Dictionary<string, string?> credentials,
        CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        Manage();
        if (!SmsGateways.IsValid(gateway))
            throw new AppException($"Unknown SMS gateway '{gateway}'.", 400);

        var s = await uow.SmsSettings.GetOrCreateAsync(cancellationToken);
        s.ActivatedGateway = gateway.Trim().ToLowerInvariant();
        s.CredentialsJson = JsonSerializer.Serialize(credentials);
        await uow.SmsSettings.UpdateAsync(s, cancellationToken);
        await uow.SaveTenantChangesAsync(cancellationToken);
        return Map(s);
    }

    public async Task<IReadOnlyList<SmsTriggerDto>> GetTriggersAsync(CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        Manage();
        var saved = (await uow.SmsSettings.GetTemplatesAsync(cancellationToken))
            .ToDictionary(t => t.EventKey, StringComparer.OrdinalIgnoreCase);

        return NotificationEventKeys.SmsDefaults.Select(d =>
        {
            saved.TryGetValue(d.Key, out var row);
            var body = row?.Body ?? d.DefaultBody;
            return new SmsTriggerDto
            {
                EventKey = d.Key,
                Name = d.Name,
                NotifyEnabled = row?.NotifyEnabled ?? true,
                NotifyStudent = row?.NotifyStudent ?? false,
                NotifyParent = row?.NotifyParent ?? true,
                DltTemplateId = row?.DltTemplateId,
                Body = body,
                CharCount = body.Length
            };
        }).ToList();
    }

    public async Task<SmsTriggerDto> UpdateTriggerAsync(string eventKey, UpdateSmsTriggerDto dto, CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        Manage();
        var def = NotificationEventKeys.SmsDefaults.FirstOrDefault(x => x.Key.Equals(eventKey, StringComparison.OrdinalIgnoreCase));
        if (def.Key is null)
            throw new NotFoundException($"Unknown SMS event key '{eventKey}'.");

        if (dto.Body.Length > 918)
            throw new AppException("SMS body exceeds maximum length (918 characters).", 400);

        await uow.SmsSettings.UpsertTemplateAsync(new SmsTemplate
        {
            EventKey = def.Key,
            Name = def.Name,
            Body = dto.Body,
            NotifyStudent = dto.NotifyStudent,
            NotifyParent = dto.NotifyParent,
            DltTemplateId = dto.DltTemplateId?.Trim(),
            NotifyEnabled = dto.NotifyEnabled
        }, cancellationToken);
        await uow.SaveTenantChangesAsync(cancellationToken);

        return new SmsTriggerDto
        {
            EventKey = def.Key,
            Name = def.Name,
            NotifyEnabled = dto.NotifyEnabled,
            NotifyStudent = dto.NotifyStudent,
            NotifyParent = dto.NotifyParent,
            DltTemplateId = dto.DltTemplateId?.Trim(),
            Body = dto.Body,
            CharCount = dto.Body.Length
        };
    }

    public async Task<SmsTestResultDto> TestSendAsync(TestSmsDto dto, CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        Manage();
        var to = dto.To.Trim();
        if (string.IsNullOrWhiteSpace(to))
            throw new AppException("Recipient number is required.", 400);

        var s = await uow.SmsSettings.GetOrCreateAsync(cancellationToken);
        if (!s.IsEnabled)
            throw new AppException("SMS gateway is disabled. Enable it in SMS config first.", 400);

        var message = string.IsNullOrWhiteSpace(dto.Message)
            ? "School Management — test SMS"
            : dto.Message.Trim();

        var sender = smsFactory.Resolve(s.ActivatedGateway, s.CredentialsJson);
        if (sender is Services.Sms.BulkSmsBdSmsSender bulk)
        {
            var result = await bulk.SendOneAsync(to, message, cancellationToken);
            return new SmsTestResultDto
            {
                Success = result.Success,
                Code = result.Code,
                Message = result.Message,
                RawResponse = result.RawResponse
            };
        }

        await sender.SendAsync(to, message, cancellationToken);
        return new SmsTestResultDto { Success = true, Message = "SMS dispatched via stub gateway." };
    }

    public async Task<SmsBalanceDto> GetBalanceAsync(CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        Manage();
        var s = await uow.SmsSettings.GetOrCreateAsync(cancellationToken);
        var sender = smsFactory.Resolve(s.ActivatedGateway, s.CredentialsJson);
        if (sender is not Services.Sms.BulkSmsBdSmsSender bulk)
            throw new AppException("Balance check is only supported for the BulkSMSBD.net gateway.", 400);

        var result = await bulk.GetBalanceAsync(cancellationToken);
        return new SmsBalanceDto
        {
            Success = result.Success,
            Balance = result.Balance,
            Message = result.Message,
            RawResponse = result.RawResponse
        };
    }

    private static SmsConfigDto Map(SmsSettings s)
    {
        Dictionary<string, string?> credentials = new();
        try
        {
            credentials = JsonSerializer.Deserialize<Dictionary<string, string?>>(s.CredentialsJson) ?? new();
        }
        catch { /* ignore */ }

        return new SmsConfigDto
        {
            IsEnabled = s.IsEnabled,
            ActivatedGateway = s.ActivatedGateway,
            Credentials = credentials,
            AvailableGateways = SmsGateways.All
        };
    }

    private async Task Ready(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(tenant.SchemaName))
            throw new AppException("X-Tenant-ID header is required.", 400);
        await provisioner.EnsureSettingsModuleAsync(tenant.SchemaName!, ct);
    }

    private HashSet<string> Roles() =>
        http.HttpContext?.User.FindAll("role").Concat(http.HttpContext.User.FindAll(ClaimTypes.Role))
            .Select(x => x.Value).ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];

    private void Manage()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin))
            throw new ForbiddenException("Only Super Admin or School Admin can manage SMS settings.");
    }
}
