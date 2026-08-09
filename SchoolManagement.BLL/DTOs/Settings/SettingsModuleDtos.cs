namespace SchoolManagement.BLL.DTOs.Settings;

public class RoleResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
    public bool IsSystemRole { get; set; }
    public bool IsActive { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateRoleDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateRoleDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}

public class RolePermissionItemDto
{
    public string FeatureKey { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool ViewOnly { get; set; }
    public bool CanView { get; set; }
    public bool CanAdd { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}

public class RolePermissionMatrixDto
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public IReadOnlyList<RolePermissionItemDto> Permissions { get; set; } = [];
}

public class UpdateRolePermissionItemDto
{
    public string FeatureKey { get; set; } = string.Empty;
    public bool CanView { get; set; }
    public bool CanAdd { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}

public class UpdateRolePermissionsDto
{
    public List<UpdateRolePermissionItemDto> Permissions { get; set; } = [];
}

public class AcademicSessionResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateAcademicSessionDto
{
    public string Name { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
}

public class UpdateAcademicSessionDto
{
    public string? Name { get; set; }
    public bool? IsSelected { get; set; }
}

public class CronSettingsResponseDto
{
    public string SecretKey { get; set; } = string.Empty;
    public string SendSmsEmailUrl { get; set; } = string.Empty;
    public string HomeworkUrl { get; set; } = string.Empty;
    public string FeesReminderUrl { get; set; } = string.Empty;
}

public class CronJobResultDto
{
    public bool Success { get; set; }
    public string Job { get; set; } = string.Empty;
    public int Processed { get; set; }
    public int Skipped { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class DatabaseBackupResponseDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DatabaseBackupListDto
{
    public IReadOnlyList<DatabaseBackupResponseDto> Data { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class DatabaseBackupDownloadDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
}

public class LoginLogItemDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string? Browser { get; set; }
    public string? Platform { get; set; }
    public DateTime LoginDateTime { get; set; }
}

public class LoginLogListDto
{
    public IReadOnlyList<LoginLogItemDto> Data { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class AttendanceTypeDto
{
    public string AttendanceType { get; set; } = "DayWise";
    public string Note { get; set; } = "This setting does not affect the SuperAdmin role.";
}

public class UpdateAttendanceTypeDto
{
    public string AttendanceType { get; set; } = "DayWise";
}

public class AccountingLinksDto
{
    public bool IsEnabled { get; set; }
    public Guid? DefaultDepositAccountId { get; set; }
    public Guid? DefaultExpenseAccountId { get; set; }
}

public class EmailConfigDto
{
    public bool IsEnabled { get; set; }
    public string? SystemEmail { get; set; }
    public string Protocol { get; set; } = "SMTP";
    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; } = 587;
    public string? SmtpUsername { get; set; }
    public bool HasPassword { get; set; }
    public string SmtpSecure { get; set; } = "TLS";
    public bool SmtpAuth { get; set; } = true;
    public string? FromName { get; set; }
}

public class UpdateEmailConfigDto
{
    public bool IsEnabled { get; set; }
    public string? SystemEmail { get; set; }
    public string Protocol { get; set; } = "SMTP";
    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; } = 587;
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    public string SmtpSecure { get; set; } = "TLS";
    public bool SmtpAuth { get; set; } = true;
    public string? FromName { get; set; }
}

public class TestEmailDto
{
    public string To { get; set; } = string.Empty;
}

public class EmailTriggerDto
{
    public string EventKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool NotifyEnabled { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
}

public class UpdateEmailTriggerDto
{
    public bool NotifyEnabled { get; set; } = true;
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
}

public class SmsConfigDto
{
    public bool IsEnabled { get; set; }
    public string ActivatedGateway { get; set; } = "bulksmsbd";
    public Dictionary<string, string?> Credentials { get; set; } = new();
    public IReadOnlyList<string> AvailableGateways { get; set; } = [];
}

public class UpdateSmsConfigDto
{
    public bool IsEnabled { get; set; }
    public string ActivatedGateway { get; set; } = "bulksmsbd";
    public Dictionary<string, string?>? Credentials { get; set; }
}

public class TestSmsDto
{
    public string To { get; set; } = string.Empty;
    public string? Message { get; set; }
}

public class SmsTestResultDto
{
    public bool Success { get; set; }
    public string? Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? RawResponse { get; set; }
}

public class SmsBalanceDto
{
    public bool Success { get; set; }
    public decimal? Balance { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? RawResponse { get; set; }
}

public class SmsTriggerDto
{
    public string EventKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool NotifyEnabled { get; set; }
    public bool NotifyStudent { get; set; }
    public bool NotifyParent { get; set; }
    public string? DltTemplateId { get; set; }
    public string Body { get; set; } = string.Empty;
    public int CharCount { get; set; }
}

public class UpdateSmsTriggerDto
{
    public bool NotifyEnabled { get; set; } = true;
    public bool NotifyStudent { get; set; }
    public bool NotifyParent { get; set; } = true;
    public string? DltTemplateId { get; set; }
    public string Body { get; set; } = string.Empty;
}
