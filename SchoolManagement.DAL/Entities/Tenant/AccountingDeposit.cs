namespace SchoolManagement.DAL.Entities.Tenant;

public class AccountingDeposit
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public Guid VoucherHeadId { get; set; }
    public string? RefNo { get; set; }
    public decimal Amount { get; set; }
    public DateTime DepositDate { get; set; } = DateTime.UtcNow.Date;
    public string? PayVia { get; set; }
    public string? Description { get; set; }
    public string? AttachmentUrl { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public AccountingAccount Account { get; set; } = null!;
    public VoucherHead VoucherHead { get; set; } = null!;
    public User? CreatedByUser { get; set; }
}
