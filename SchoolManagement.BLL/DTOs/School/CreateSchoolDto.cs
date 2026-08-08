namespace SchoolManagement.BLL.DTOs.School;

public class CreateSchoolDto
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Domain { get; set; }
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
    public string? Locale { get; set; } = "en-US";
    public int? EstablishedYear { get; set; }
    public string? SchoolType { get; set; }
    public string SubscriptionPlan { get; set; } = "basic";
    public DateTime? SubscriptionExpiresAt { get; set; }
    public int MaxUsers { get; set; } = 100;

    public string AdminEmail { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
    public string AdminFirstName { get; set; } = string.Empty;
    public string AdminLastName { get; set; } = string.Empty;
}

public class UpdateSchoolDto
{
    public string? Name { get; set; }
    public string? Domain { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? Currency { get; set; }
    public string? CurrencySymbol { get; set; }
    public string? Timezone { get; set; }
    public string? Locale { get; set; }
    public int? EstablishedYear { get; set; }
    public string? SchoolType { get; set; }
    public string? SubscriptionPlan { get; set; }
    public DateTime? SubscriptionExpiresAt { get; set; }
    public int? MaxUsers { get; set; }
}

public class SchoolResponseDto
{
    public Guid Id { get; set; }
    public int Sl { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string SchemaName { get; set; } = string.Empty;
    public string? Domain { get; set; }
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
    public string? Locale { get; set; }
    public string? LogoUrl { get; set; }
    public int? EstablishedYear { get; set; }
    public string? SchoolType { get; set; }
    public string SubscriptionPlan { get; set; } = "basic";
    public DateTime? SubscriptionExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public int MaxUsers { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SchoolListResponseDto
{
    public IReadOnlyList<SchoolResponseDto> Items { get; set; } = Array.Empty<SchoolResponseDto>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}

public class SchoolSettingsDto
{
    public FeatureSettings Features { get; set; } = new();
    public BrandingSettings Branding { get; set; } = new();
    public SecuritySettings Security { get; set; } = new();

    public class FeatureSettings
    {
        public int MaxUsers { get; set; } = 100;
        public int StorageQuotaGB { get; set; } = 10;
        public bool AllowSelfRegistration { get; set; } = false;
        public bool RequireEmailVerification { get; set; } = true;
    }

    public class BrandingSettings
    {
        public string? SchoolName { get; set; }
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
}

public class SchoolStatsDto
{
    public string Slug { get; set; } = string.Empty;
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public long StorageUsedBytes { get; set; }
    public double StorageUsedMB { get; set; }
    public int StorageQuotaGB { get; set; }
    public string SubscriptionPlan { get; set; } = string.Empty;
    public DateTime? SubscriptionExpiresAt { get; set; }
    public string SubscriptionStatus { get; set; } = "active";
}
