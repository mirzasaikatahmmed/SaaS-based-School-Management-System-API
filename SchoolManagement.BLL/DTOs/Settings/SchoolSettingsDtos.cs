namespace SchoolManagement.BLL.DTOs.Settings;

public class SchoolListItemDto
{
    public Guid Id { get; set; }
    public int Sl { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string SchemaName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string SubscriptionPlan { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class SchoolListResponseDto
{
    public IReadOnlyList<SchoolListItemDto> Data { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class SchoolSettingsResponseDto
{
    public Guid Id { get; set; }
    public string Branch { get; set; } = string.Empty;

    // General
    public string? SchoolName { get; set; }
    public string? SchoolCode { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string Timezone { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string CurrencySymbol { get; set; } = string.Empty;
    public string DateFormat { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;

    // Student panel
    public bool AllowStudentLogin { get; set; }
    public bool AllowGuardianLogin { get; set; }
    public bool ShowFeesInStudentPanel { get; set; }
    public bool ShowAttendanceInStudentPanel { get; set; }
    public bool ShowResultInStudentPanel { get; set; }
    public string? StudentPanelNoticeMessage { get; set; }

    // Logos
    public string? SystemLogoUrl { get; set; }
    public string? TextLogoUrl { get; set; }
    public string? PrintingLogoUrl { get; set; }
    public string? ReportCardLogoUrl { get; set; }

    // Payment
    public List<PaymentGatewayConfigDto> PaymentGateways { get; set; } = [];
    public List<string> ActiveGateways { get; set; } = [];

    public DateTime UpdatedAt { get; set; }
}

public class UpdateSchoolGeneralDto
{
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
}

public class UpdateStudentPanelDto
{
    public bool AllowStudentLogin { get; set; } = true;
    public bool AllowGuardianLogin { get; set; } = true;
    public bool ShowFeesInStudentPanel { get; set; } = true;
    public bool ShowAttendanceInStudentPanel { get; set; } = true;
    public bool ShowResultInStudentPanel { get; set; } = true;
    public string? StudentPanelNoticeMessage { get; set; }
}

public class PaymentGatewayConfigDto
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public Dictionary<string, string?> Config { get; set; } = new();
}

public class PaymentSettingsDto
{
    public List<PaymentGatewayConfigDto> Gateways { get; set; } = [];
}
