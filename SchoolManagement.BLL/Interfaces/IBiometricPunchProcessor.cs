using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.BLL.Interfaces;

/// <summary>
/// Shared punch-processing pipeline used by both the ZKTeco ADMS controller (device pushes)
/// and the manual "test a punch" admin endpoint. Always operates against the tenant
/// already resolved on <see cref="SchoolManagement.DAL.TenantContext.ITenantContext"/>.
/// </summary>
public interface IBiometricPunchProcessor
{
    Task<BiometricPunchLog> ProcessPunchAsync(
        Guid? deviceId,
        string deviceSn,
        int graceMinutesBefore,
        int graceMinutesAfter,
        string devicePin,
        DateTime punchTime,
        string? rawLine,
        CancellationToken cancellationToken = default);
}
