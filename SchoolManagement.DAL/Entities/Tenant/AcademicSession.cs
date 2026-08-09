namespace SchoolManagement.DAL.Entities.Tenant;

/// <summary>Academic year/session labels (e.g. "2025", "2025-2026"). One session is flagged current.</summary>
public class AcademicSession
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
