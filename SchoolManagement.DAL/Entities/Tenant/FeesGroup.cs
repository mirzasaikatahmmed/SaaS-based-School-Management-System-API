namespace SchoolManagement.DAL.Entities.Tenant;

public class FeesGroup
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<FeesGroupItem> Items { get; set; } = new List<FeesGroupItem>();
    public ICollection<FeesAllocation> Allocations { get; set; } = new List<FeesAllocation>();
}
