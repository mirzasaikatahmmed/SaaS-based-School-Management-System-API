namespace SchoolManagement.DAL.Entities.Tenant;

public class EmployeeAttendance
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public string? Status { get; set; }
    public string? Remarks { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Employee Employee { get; set; } = null!;
    public User? CreatedByUser { get; set; }
}
