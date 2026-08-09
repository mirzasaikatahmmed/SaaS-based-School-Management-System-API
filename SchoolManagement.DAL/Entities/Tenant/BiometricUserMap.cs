namespace SchoolManagement.DAL.Entities.Tenant;

public class BiometricUserMap
{
    public Guid Id { get; set; }
    /// <summary>PIN / badge ID enrolled on the ZKTeco device.</summary>
    public string DevicePin { get; set; } = string.Empty;
    public string PersonType { get; set; } = "Student"; // Student | Employee
    public Guid? StudentId { get; set; }
    public Guid? EmployeeId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Student? Student { get; set; }
    public Employee? Employee { get; set; }
}
