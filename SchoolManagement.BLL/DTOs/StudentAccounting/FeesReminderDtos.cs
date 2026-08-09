namespace SchoolManagement.BLL.DTOs.StudentAccounting;

public class CreateFeesReminderDto
{
    public string Frequency { get; set; } = string.Empty;
    public int Days { get; set; }
    public string? Message { get; set; }
    public string? DltTemplateId { get; set; }
    public bool NotifyStudent { get; set; }
    public bool NotifyGuardian { get; set; }
}

public class UpdateFeesReminderDto
{
    public string Frequency { get; set; } = string.Empty;
    public int Days { get; set; }
    public string? Message { get; set; }
    public string? DltTemplateId { get; set; }
    public bool NotifyStudent { get; set; }
    public bool NotifyGuardian { get; set; }
    public bool? IsActive { get; set; }
}

public class FeesReminderResponseDto
{
    public Guid Id { get; set; }
    public string Frequency { get; set; } = string.Empty;
    public int Days { get; set; }
    public string? Message { get; set; }
    public string? DltTemplateId { get; set; }
    public bool NotifyStudent { get; set; }
    public bool NotifyGuardian { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
