namespace SchoolManagement.DAL.Entities.Master;

/// <summary>
/// Platform-wide settings row in public.global_settings — single row, master DB only.
/// No tenant schema is required to read/update these values.
/// </summary>
public class GlobalSettings
{
    public Guid Id { get; set; }

    // General
    public string SiteName { get; set; } = "School Management System";
    public string? SiteTitle { get; set; }
    public string? SiteLogoUrl { get; set; }
    public string? SiteFaviconUrl { get; set; }
    public string? AdminEmail { get; set; }
    public string? SupportPhone { get; set; }
    public string DefaultTimezone { get; set; } = "Asia/Dhaka";
    public string DefaultCurrency { get; set; } = "BDT";
    public string DefaultCurrencySymbol { get; set; } = "৳";
    public string DefaultLocale { get; set; } = "en-US";
    public string DefaultDateFormat { get; set; } = "DD/MM/YYYY";
    public bool MaintenanceMode { get; set; }
    public string? MaintenanceMessage { get; set; }

    // Upload file settings
    public int MaxUploadSizeMb { get; set; } = 5;
    public string AllowedFileTypes { get; set; } = "jpg,jpeg,png,gif,pdf,doc,docx,xls,xlsx,csv";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
