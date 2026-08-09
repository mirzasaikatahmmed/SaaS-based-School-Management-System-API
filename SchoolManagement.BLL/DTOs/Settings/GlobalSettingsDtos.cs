namespace SchoolManagement.BLL.DTOs.Settings;

public class GlobalSettingsResponseDto
{
    public Guid Id { get; set; }
    public string SiteName { get; set; } = string.Empty;
    public string? SiteTitle { get; set; }
    public string? SiteLogoUrl { get; set; }
    public string? SiteFaviconUrl { get; set; }
    public string? AdminEmail { get; set; }
    public string? SupportPhone { get; set; }
    public string DefaultTimezone { get; set; } = string.Empty;
    public string DefaultCurrency { get; set; } = string.Empty;
    public string DefaultCurrencySymbol { get; set; } = string.Empty;
    public string DefaultLocale { get; set; } = string.Empty;
    public string DefaultDateFormat { get; set; } = string.Empty;
    public bool MaintenanceMode { get; set; }
    public string? MaintenanceMessage { get; set; }
    public int MaxUploadSizeMb { get; set; }
    public string AllowedFileTypes { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}

public class UpdateGlobalGeneralDto
{
    public string SiteName { get; set; } = string.Empty;
    public string? SiteTitle { get; set; }
    public string? AdminEmail { get; set; }
    public string? SupportPhone { get; set; }
    public string DefaultTimezone { get; set; } = "Asia/Dhaka";
    public string DefaultCurrency { get; set; } = "BDT";
    public string DefaultCurrencySymbol { get; set; } = "৳";
    public string DefaultLocale { get; set; } = "en-US";
    public string DefaultDateFormat { get; set; } = "DD/MM/YYYY";
    public bool MaintenanceMode { get; set; }
    public string? MaintenanceMessage { get; set; }
}

public class UpdateGlobalUploadFileDto
{
    public int MaxUploadSizeMb { get; set; } = 5;
    public string AllowedFileTypes { get; set; } = "jpg,jpeg,png,gif,pdf,doc,docx,xls,xlsx,csv";
}
