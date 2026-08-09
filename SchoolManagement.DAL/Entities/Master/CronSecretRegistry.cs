namespace SchoolManagement.DAL.Entities.Master;

/// <summary>
/// Maps a per-tenant cron secret key to its schema so the anonymous `/cron_api/*` endpoints
/// (no X-Tenant-ID) can resolve which tenant schema to operate on, mirroring
/// <see cref="BiometricDeviceRegistry"/>'s SN-to-tenant lookup.
/// </summary>
public class CronSecretRegistry
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string SchemaName { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Tenant? Tenant { get; set; }
}
