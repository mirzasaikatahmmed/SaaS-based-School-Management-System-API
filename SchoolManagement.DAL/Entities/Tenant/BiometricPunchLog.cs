namespace SchoolManagement.DAL.Entities.Tenant;

public class BiometricPunchLog
{
    public Guid Id { get; set; }
    public Guid? DeviceId { get; set; }
    public string DeviceSn { get; set; } = string.Empty;
    public string DevicePin { get; set; } = string.Empty;
    public DateTime PunchTime { get; set; }
    public string PunchKind { get; set; } = "Unmapped"; // StudentDaily | EmployeeDaily | Exam | Unmapped
    public string StatusApplied { get; set; } = "Present";
    public Guid? StudentId { get; set; }
    public Guid? EmployeeId { get; set; }
    public Guid? ExamId { get; set; }
    public Guid? SubjectId { get; set; }
    public string? RawLine { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public BiometricDevice? Device { get; set; }
    public Student? Student { get; set; }
    public Employee? Employee { get; set; }
}
