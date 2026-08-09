namespace SchoolManagement.DAL.Entities.Tenant;

public class FeesGroupItem
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public Guid FeesTypeId { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public FeesGroup Group { get; set; } = null!;
    public FeesType FeesType { get; set; } = null!;
}
