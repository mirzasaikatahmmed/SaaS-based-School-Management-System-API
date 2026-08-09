namespace SchoolManagement.DAL.Entities.Tenant;

public class BiometricDevice
{
    public Guid Id { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string DeviceModel { get; set; } = "K40-H";
    public int ExamGraceMinutesBefore { get; set; } = 30;
    public int ExamGraceMinutesAfter { get; set; } = 30;
    public bool IsActive { get; set; } = true;
    public DateTime? LastSeenAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<BiometricPunchLog> PunchLogs { get; set; } = new List<BiometricPunchLog>();
}
