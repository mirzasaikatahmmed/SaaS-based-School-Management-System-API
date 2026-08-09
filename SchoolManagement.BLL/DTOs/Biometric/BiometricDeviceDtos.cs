namespace SchoolManagement.BLL.DTOs.Biometric;

public class CreateDeviceDto
{
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string? DeviceModel { get; set; } = "K40-H";
    public int ExamGraceMinutesBefore { get; set; } = 30;
    public int ExamGraceMinutesAfter { get; set; } = 30;
}

public class UpdateDeviceDto
{
    public string? Name { get; set; }
    public string? Location { get; set; }
    public int? ExamGraceMinutesBefore { get; set; }
    public int? ExamGraceMinutesAfter { get; set; }
    public bool? IsActive { get; set; }
}

public class DeviceResponseDto
{
    public Guid Id { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string DeviceModel { get; set; } = string.Empty;
    public int ExamGraceMinutesBefore { get; set; }
    public int ExamGraceMinutesAfter { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
