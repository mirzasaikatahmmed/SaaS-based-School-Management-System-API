namespace SchoolManagement.DAL.Entities.Tenant;

public class LeaveCategory
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int Days { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
}
