namespace SchoolManagement.DAL.Entities.Tenant;

/// <summary>Maps to table events (C# type name avoids clash with System.Event).</summary>
public class SchoolEvent
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid? EventTypeId { get; set; }
    public bool IsHoliday { get; set; }
    public string Audience { get; set; } = "Everybody";
    public DateTime DateOfStart { get; set; }
    public DateTime DateOfEnd { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool ShowWebsite { get; set; }
    public bool IsPublished { get; set; }
    public Guid? CreatedBy { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public EventType? EventType { get; set; }
    public User? CreatedByUser { get; set; }
}
