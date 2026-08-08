namespace SchoolManagement.BLL.DTOs.StudentList;

public class StudentListItemDto
{
    public Guid Id { get; set; }
    public bool IsSelected { get; set; } = false;
    public string? PhotoUrl { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public string RegisterNo { get; set; } = string.Empty;
    public string? Roll { get; set; }
    public string? DateOfBirth { get; set; }
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public string? FatherName { get; set; }
    public string? MotherName { get; set; }
    public string? GuardianMobileNo { get; set; }
    public bool IsActive { get; set; }
    public bool IsLoginActive { get; set; }
    public string? DeactivateReason { get; set; }
    public DateTime? DeactivatedAt { get; set; }
}
