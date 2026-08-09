namespace SchoolManagement.DAL.Entities.Tenant;

public class FeesReminder
{
    public Guid Id { get; set; }
    public string Frequency { get; set; } = string.Empty;
    public int Days { get; set; }
    public string? Message { get; set; }
    public string? DltTemplateId { get; set; }
    public bool NotifyStudent { get; set; }
    public bool NotifyGuardian { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
