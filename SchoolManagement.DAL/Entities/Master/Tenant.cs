using System.Text.Json;

namespace SchoolManagement.DAL.Entities.Master;

/// <summary>
/// Master registry row in public.tenants — each School is one SaaS tenant.
/// Column naming follows ahskbera_main branch/global_settings conventions where applicable.
/// </summary>
public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Domain { get; set; }
    public string SchemaName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string SubscriptionPlan { get; set; } = "basic";
    public int MaxUsers { get; set; } = 100;
    public string Settings { get; set; } = "{}";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // School-specific fields (ahskbera branch / global_settings aligned)
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string Country { get; set; } = "Bangladesh";
    public string Currency { get; set; } = "BDT";
    public string CurrencySymbol { get; set; } = "৳";
    public string Timezone { get; set; } = "Asia/Dhaka";
    public string Locale { get; set; } = "en-US";
    public string? LogoUrl { get; set; }
    public int? EstablishedYear { get; set; }
    public string? SchoolType { get; set; }
    public DateTime? SubscriptionExpiresAt { get; set; }

    public TenantSettings GetSettings()
    {
        try
        {
            return JsonSerializer.Deserialize<TenantSettings>(Settings) ?? new TenantSettings();
        }
        catch
        {
            return new TenantSettings();
        }
    }

    public void SetSettings(TenantSettings settings)
    {
        Settings = JsonSerializer.Serialize(settings);
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>Domain alias — School == Tenant row in public.tenants.</summary>
public class School : Tenant
{
}

public class TenantSettings
{
    public FeatureSettings Features { get; set; } = new();
    public BrandingSettings Branding { get; set; } = new();
    public SecuritySettings Security { get; set; } = new();
}

public class FeatureSettings
{
    public int MaxUsers { get; set; } = 100;
    public int StorageQuotaGB { get; set; } = 10;
    public bool AllowSelfRegistration { get; set; } = false;
    public bool RequireEmailVerification { get; set; } = true;
}

public class BrandingSettings
{
    public string SchoolName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string PrimaryColor { get; set; } = "#1a73e8";
    public string Timezone { get; set; } = "Asia/Dhaka";
    public string Locale { get; set; } = "en-US";
}

public class SecuritySettings
{
    public int PasswordMinLength { get; set; } = 8;
    public int SessionTimeoutMinutes { get; set; } = 60;
    public int MaxLoginAttempts { get; set; } = 5;
    public int LockoutDurationMinutes { get; set; } = 15;
}

public class SuperAdmin
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
