using SchoolManagement.BLL.DTOs.Settings;

namespace SchoolManagement.BLL.Interfaces;

public interface IRoleService
{
    Task<IReadOnlyList<RoleResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RoleResponseDto> CreateAsync(CreateRoleDto dto, CancellationToken cancellationToken = default);
    Task<RoleResponseDto> UpdateAsync(Guid id, UpdateRoleDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RolePermissionMatrixDto> GetPermissionsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RolePermissionMatrixDto> UpdatePermissionsAsync(Guid id, UpdateRolePermissionsDto dto, CancellationToken cancellationToken = default);
}

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(Guid userId, IEnumerable<string> rolePrefixes, string featureKey, string action, CancellationToken cancellationToken = default);
}

public interface IAcademicSessionService
{
    Task<IReadOnlyList<AcademicSessionResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AcademicSessionResponseDto?> GetCurrentAsync(CancellationToken cancellationToken = default);
    Task<AcademicSessionResponseDto> CreateAsync(CreateAcademicSessionDto dto, CancellationToken cancellationToken = default);
    Task<AcademicSessionResponseDto> UpdateAsync(Guid id, UpdateAcademicSessionDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface ICronSettingsService
{
    Task<CronSettingsResponseDto> GetAsync(CancellationToken cancellationToken = default);
    Task<CronSettingsResponseDto> RegenerateKeyAsync(CancellationToken cancellationToken = default);
    Task<CronJobResultDto> RunSendSmsEmailAsync(string secretKey, CancellationToken cancellationToken = default);
    Task<CronJobResultDto> RunHomeworkAsync(string secretKey, CancellationToken cancellationToken = default);
    Task<CronJobResultDto> RunFeesReminderAsync(string secretKey, CancellationToken cancellationToken = default);
}

public interface IDatabaseBackupService
{
    Task<DatabaseBackupListDto> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<DatabaseBackupResponseDto> CreateAsync(CancellationToken cancellationToken = default);
    Task<DatabaseBackupDownloadDto> GetDownloadAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task RestoreAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
}

public interface IUserLoginLogService
{
    Task<LoginLogListDto> GetAsync(string? type, string? search, int page, int pageSize, string? export, CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);
}

public interface INotificationTemplateService
{
    string Render(string template, IReadOnlyDictionary<string, string?> data);
}

public interface IEmailSender
{
    Task SendAsync(string to, string subject, string bodyHtml, CancellationToken cancellationToken = default);
}

public interface ISmsSender
{
    Task SendAsync(string to, string body, CancellationToken cancellationToken = default);
}

public interface ISmsSenderFactory
{
    ISmsSender Resolve(string gateway, string credentialsJson);
}

public interface IEmailSettingsAppService
{
    Task<EmailConfigDto> GetConfigAsync(CancellationToken cancellationToken = default);
    Task<EmailConfigDto> UpdateConfigAsync(UpdateEmailConfigDto dto, CancellationToken cancellationToken = default);
    Task TestAsync(TestEmailDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmailTriggerDto>> GetTriggersAsync(CancellationToken cancellationToken = default);
    Task<EmailTriggerDto> UpdateTriggerAsync(string eventKey, UpdateEmailTriggerDto dto, CancellationToken cancellationToken = default);
}

public interface ISmsSettingsAppService
{
    Task<SmsConfigDto> GetConfigAsync(CancellationToken cancellationToken = default);
    Task<SmsConfigDto> UpdateConfigAsync(UpdateSmsConfigDto dto, CancellationToken cancellationToken = default);
    Task<SmsConfigDto> UpdateGatewayCredentialsAsync(string gateway, Dictionary<string, string?> credentials, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SmsTriggerDto>> GetTriggersAsync(CancellationToken cancellationToken = default);
    Task<SmsTriggerDto> UpdateTriggerAsync(string eventKey, UpdateSmsTriggerDto dto, CancellationToken cancellationToken = default);
    Task<SmsTestResultDto> TestSendAsync(TestSmsDto dto, CancellationToken cancellationToken = default);
    Task<SmsBalanceDto> GetBalanceAsync(CancellationToken cancellationToken = default);
}

public interface ISchoolSettingsExtrasService
{
    Task<AttendanceTypeDto> GetAttendanceTypeAsync(string slug, CancellationToken cancellationToken = default);
    Task<AttendanceTypeDto> UpdateAttendanceTypeAsync(string slug, UpdateAttendanceTypeDto dto, CancellationToken cancellationToken = default);
    Task<AccountingLinksDto> GetAccountingLinksAsync(string slug, CancellationToken cancellationToken = default);
    Task<AccountingLinksDto> UpdateAccountingLinksAsync(string slug, AccountingLinksDto dto, CancellationToken cancellationToken = default);
}
