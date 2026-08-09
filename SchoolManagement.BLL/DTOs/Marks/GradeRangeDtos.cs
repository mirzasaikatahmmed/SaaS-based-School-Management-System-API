namespace SchoolManagement.BLL.DTOs.Marks;

public class CreateGradeRangeDto
{
    public string GradeName { get; set; } = string.Empty;
    public decimal GradePoint { get; set; }
    public decimal MinPercentage { get; set; }
    public decimal MaxPercentage { get; set; }
    public string? Remarks { get; set; }
    public int SortOrder { get; set; }
}

public class UpdateGradeRangeDto
{
    public string GradeName { get; set; } = string.Empty;
    public decimal GradePoint { get; set; }
    public decimal MinPercentage { get; set; }
    public decimal MaxPercentage { get; set; }
    public string? Remarks { get; set; }
    public int SortOrder { get; set; }
    public bool? IsActive { get; set; }
}

public class GradeRangeDto
{
    public Guid Id { get; set; }
    public string Branch { get; set; } = string.Empty;
    public string GradeName { get; set; } = string.Empty;
    public decimal GradePoint { get; set; }
    public decimal MinPercentage { get; set; }
    public decimal MaxPercentage { get; set; }
    public string? Remarks { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}
