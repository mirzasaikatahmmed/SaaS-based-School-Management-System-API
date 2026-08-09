namespace SchoolManagement.DAL.Entities.Tenant;

public class OfflinePayment
{
    public Guid Id { get; set; }
    public string TrxId { get; set; } = string.Empty;
    public Guid StudentId { get; set; }
    public Guid? PaymentTypeId { get; set; }
    public Guid? ClassId { get; set; }
    public Guid? SectionId { get; set; }
    public DateTime PaymentDate { get; set; }
    public DateTime SubmitDate { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Student Student { get; set; } = null!;
    public OfflinePaymentType? PaymentType { get; set; }
    public ClassEntity? Class { get; set; }
    public Section? Section { get; set; }
}
