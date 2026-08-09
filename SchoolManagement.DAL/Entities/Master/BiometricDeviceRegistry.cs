namespace SchoolManagement.DAL.Entities.Master;

/// <summary>Maps ZKTeco device serial numbers to a tenant (ADMS has no X-Tenant-ID).</summary>
public class BiometricDeviceRegistry
{
    public Guid Id { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string SchemaName { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string AttLogStamp { get; set; } = "0";
    public string OperLogStamp { get; set; } = "0";
    public DateTime? LastSeenAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Tenant? Tenant { get; set; }
}
