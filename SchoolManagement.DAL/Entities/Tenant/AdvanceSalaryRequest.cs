namespace SchoolManagement.DAL.Entities.Tenant;

public class AdvanceSalaryRequest
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string DeductMonth { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = HrRequestStatuses.Pending;
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? RejectReason { get; set; }
    public DateTime AppliedOn { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Employee Employee { get; set; } = null!;
    public User? Reviewer { get; set; }
}

public static class HrRequestStatuses
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
}
