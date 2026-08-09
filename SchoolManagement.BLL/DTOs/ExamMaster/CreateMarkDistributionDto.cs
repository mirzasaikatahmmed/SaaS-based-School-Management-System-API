namespace SchoolManagement.BLL.DTOs.ExamMaster;

public class CreateMarkDistributionDto
{
    public string Name { get; set; } = string.Empty;
}

public class UpdateMarkDistributionDto
{
    public string Name { get; set; } = string.Empty;
    public bool? IsActive { get; set; }
}

public class MarkDistributionResponseDto
{
    public Guid Id { get; set; }
    public int Sl { get; set; }
    public string Branch { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
