namespace SchoolManagement.DAL.Entities.Tenant;

public class GradeRange
{
    public Guid Id { get; set; }
    public string GradeName { get; set; } = string.Empty;
    public decimal GradePoint { get; set; }
    public decimal MinPercentage { get; set; }
    public decimal MaxPercentage { get; set; }
    public string? Remarks { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
