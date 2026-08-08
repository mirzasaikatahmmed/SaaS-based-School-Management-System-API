using SchoolManagement.DAL.Entities.Master;

namespace SchoolManagement.BLL.DTOs.Tenant;

public class CreateTenantDto
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Domain { get; set; }
    public string SubscriptionPlan { get; set; } = "basic";
    public int MaxUsers { get; set; } = 100;
    public TenantSettingsDto? Settings { get; set; }
    public CreateTenantAdminDto? Admin { get; set; }
}

public class CreateTenantAdminDto
{
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class UpdateTenantSettingsDto
{
    public string? Name { get; set; }
    public string? Domain { get; set; }
    public string? SubscriptionPlan { get; set; }
    public int? MaxUsers { get; set; }
    public TenantSettingsDto? Settings { get; set; }
}

public class TenantResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Domain { get; set; }
    public string SchemaName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string SubscriptionPlan { get; set; } = string.Empty;
    public int MaxUsers { get; set; }
    public TenantSettingsDto Settings { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class TenantSettingsDto
{
    public FeatureSettingsDto Features { get; set; } = new();
    public BrandingSettingsDto Branding { get; set; } = new();
    public SecuritySettingsDto Security { get; set; } = new();
}

public class FeatureSettingsDto
{
    public int MaxUsers { get; set; } = 100;
    public int StorageQuotaGB { get; set; } = 10;
    public bool AllowSelfRegistration { get; set; } = true;
    public bool RequireEmailVerification { get; set; } = true;
}

public class BrandingSettingsDto
{
    public string SchoolName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string PrimaryColor { get; set; } = "#1a73e8";
    public string Timezone { get; set; } = "UTC";
    public string Locale { get; set; } = "en-US";
}

public class SecuritySettingsDto
{
    public int PasswordMinLength { get; set; } = 8;
    public int SessionTimeoutMinutes { get; set; } = 60;
    public int MaxLoginAttempts { get; set; } = 5;
    public int LockoutDurationMinutes { get; set; } = 15;
}
