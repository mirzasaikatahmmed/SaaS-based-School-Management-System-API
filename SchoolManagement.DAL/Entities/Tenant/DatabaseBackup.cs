namespace SchoolManagement.DAL.Entities.Tenant;

/// <summary>Metadata for a tenant-schema database backup stored in MinIO (`db-backups/`).</summary>
public class DatabaseBackup
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ObjectKey { get; set; } = string.Empty;
    public long SizeBytes { get; set; }

    /// <summary>Set when pg_dump was unavailable and a placeholder schema export was used instead.</summary>
    public string? Note { get; set; }

    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
