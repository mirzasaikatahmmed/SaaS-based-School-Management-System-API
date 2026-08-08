namespace SchoolManagement.BLL.DTOs.StudentDetails;

public class DeactivateReasonDto
{
    public Guid Id { get; set; }
    public string Branch { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
