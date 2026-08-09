namespace SchoolManagement.BLL.DTOs.Events;

public class CreateEventTypeDto
{
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
}

public class UpdateEventTypeDto
{
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public bool? IsActive { get; set; }
}

public class EventTypeDto
{
    public Guid Id { get; set; }
    public string Branch { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public bool IsActive { get; set; }
    public int EventCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
