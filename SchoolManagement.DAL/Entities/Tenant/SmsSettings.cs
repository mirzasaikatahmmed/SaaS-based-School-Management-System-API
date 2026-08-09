namespace SchoolManagement.DAL.Entities.Tenant;

/// <summary>Single-row per tenant SMS gateway configuration.</summary>
public class SmsSettings
{
    public Guid Id { get; set; }
    public bool IsEnabled { get; set; }

    /// <summary>Gateway key selecting the <c>ISmsSender</c> implementation via the factory.</summary>
    public string ActivatedGateway { get; set; } = SmsGateways.BulksmsbdNet;

    /// <summary>Gateway-specific credentials stored as a JSON object (API key, sender id, etc.).</summary>
    public string CredentialsJson { get; set; } = "{}";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>Per-event SMS template; placeholders use <c>{key}</c> syntax rendered by INotificationTemplateService.</summary>
public class SmsTemplate
{
    public Guid Id { get; set; }
    public string EventKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool NotifyStudent { get; set; }
    public bool NotifyParent { get; set; } = true;
    public string? DltTemplateId { get; set; }
    public bool NotifyEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public static class SmsGateways
{
    public const string Twilio = "twilio";
    public const string Clickatell = "clickatell";
    public const string Msg91 = "msg91";
    public const string Bulk = "bulk";
    public const string TextLocal = "textlocal";
    public const string SmsCountry = "smscountry";
    public const string BulksmsbdNet = "bulksmsbd";
    public const string Custom = "custom";

    public static readonly string[] All =
    [
        Twilio, Clickatell, Msg91, Bulk, TextLocal, SmsCountry, BulksmsbdNet, Custom
    ];

    public static bool IsValid(string? gateway) =>
        All.Any(x => x.Equals(gateway?.Trim(), StringComparison.OrdinalIgnoreCase));
}
