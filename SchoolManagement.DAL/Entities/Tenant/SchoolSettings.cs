namespace SchoolManagement.DAL.Entities.Tenant;

/// <summary>
/// Per-tenant settings row — single row per tenant schema.
/// PaymentGateways/ActiveGateways are stored as JSON strings mapped to jsonb columns
/// (same pattern as Tenant.Settings in the master DB).
/// </summary>
public class SchoolSettings
{
    public Guid Id { get; set; }

    // General
    public string? SchoolName { get; set; }
    public string? SchoolCode { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string Timezone { get; set; } = "Asia/Dhaka";
    public string Currency { get; set; } = "BDT";
    public string CurrencySymbol { get; set; } = "৳";
    public string DateFormat { get; set; } = "DD/MM/YYYY";
    public string Language { get; set; } = "en";

    // Student panel
    public bool AllowStudentLogin { get; set; } = true;
    public bool AllowGuardianLogin { get; set; } = true;
    public bool ShowFeesInStudentPanel { get; set; } = true;
    public bool ShowAttendanceInStudentPanel { get; set; } = true;
    public bool ShowResultInStudentPanel { get; set; } = true;
    public string? StudentPanelNoticeMessage { get; set; }

    // Logos
    public string? SystemLogoUrl { get; set; }
    public string? TextLogoUrl { get; set; }
    public string? PrintingLogoUrl { get; set; }
    public string? ReportCardLogoUrl { get; set; }

    // Payment — JSON stored as jsonb
    public string PaymentGateways { get; set; } = "{}";
    public string ActiveGateways { get; set; } = "[]";

    // Attendance
    public string AttendanceType { get; set; } = AttendanceTypes.DayWise;
    /// <summary>
    /// Comma-separated <see cref="DayOfWeek"/> ints (Sunday=0 … Saturday=6). Default Fri+Sat for BD schools.
    /// </summary>
    public string WeekendDays { get; set; } = "5,6";

    // Accounting links
    public Guid? DefaultDepositAccountId { get; set; }
    public Guid? DefaultExpenseAccountId { get; set; }
    public bool AccountingLinksEnabled { get; set; }

    // Cron
    public string? CronSecretKey { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public static class AttendanceTypes
{
    public const string DayWise = "DayWise";
    public const string SubjectWise = "SubjectWise";
    public static readonly string[] All = [DayWise, SubjectWise];
    public static bool IsValid(string? type) => All.Any(x => x.Equals(type?.Trim(), StringComparison.OrdinalIgnoreCase));
}

public static class LogoTypes
{
    public const string System = "system";
    public const string Text = "text";
    public const string Printing = "printing";
    public const string ReportCard = "report-card";

    public static readonly string[] All = [System, Text, Printing, ReportCard];

    public static bool IsValid(string? type) =>
        All.Any(x => x.Equals(type?.Trim(), StringComparison.OrdinalIgnoreCase));
}
