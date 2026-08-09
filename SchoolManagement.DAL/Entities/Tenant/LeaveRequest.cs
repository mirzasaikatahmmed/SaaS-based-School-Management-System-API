namespace SchoolManagement.DAL.Entities.Tenant;

public class LeaveRequest
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid LeaveCategoryId { get; set; }
    public DateTime DateOfStart { get; set; }
    public DateTime DateOfEnd { get; set; }
    public int Days { get; set; }
    public string? Reason { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? Comments { get; set; }
    public string Status { get; set; } = HrRequestStatuses.Pending;
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime ApplyDate { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Employee Employee { get; set; } = null!;
    public LeaveCategory LeaveCategory { get; set; } = null!;
    public User? Reviewer { get; set; }
}
