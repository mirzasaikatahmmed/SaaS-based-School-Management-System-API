namespace SchoolManagement.DAL.Entities.Tenant;

public class FineSetup
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public Guid FeesTypeId { get; set; }
    public string FineType { get; set; } = string.Empty;
    public decimal FineValue { get; set; }
    public string? LateFeeFrequency { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public FeesGroup Group { get; set; } = null!;
    public FeesType FeesType { get; set; } = null!;
}
