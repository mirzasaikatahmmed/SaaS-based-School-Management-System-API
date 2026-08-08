namespace SchoolManagement.BLL.DTOs.OnlineAdmission;

public class ApproveAdmissionDto
{
    public string? RegisterNo { get; set; }
    public string? Roll { get; set; }
    public Guid? SectionId { get; set; }
    public string AdminUsername { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
}
