namespace SchoolManagement.DAL.Entities.Tenant;

public class StudentFeeInvoice
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Guid FeesAllocationId { get; set; }
    public Guid FeesGroupId { get; set; }
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal FineAmount { get; set; }
    public decimal DueAmount { get; set; }
    public string Status { get; set; } = "Unpaid";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Student Student { get; set; } = null!;
    public FeesAllocation FeesAllocation { get; set; } = null!;
    public FeesGroup FeesGroup { get; set; } = null!;
    public ClassEntity Class { get; set; } = null!;
    public Section Section { get; set; } = null!;
}
