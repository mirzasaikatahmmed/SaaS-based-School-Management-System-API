namespace SchoolManagement.DAL.Entities.Tenant;

public class FeesAllocation
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public Guid FeesGroupId { get; set; }
    public int AcademicYear { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ClassEntity Class { get; set; } = null!;
    public Section Section { get; set; } = null!;
    public FeesGroup FeesGroup { get; set; } = null!;
    public ICollection<StudentFeeInvoice> Invoices { get; set; } = new List<StudentFeeInvoice>();
}
