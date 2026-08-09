namespace SchoolManagement.BLL.DTOs.Biometric;

public class PunchLogFilterDto
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public Guid? DeviceId { get; set; }
    public string? Kind { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class PunchLogItemDto
{
    public Guid Id { get; set; }
    public Guid? DeviceId { get; set; }
    public string DeviceSn { get; set; } = string.Empty;
    public string? DeviceName { get; set; }
    public string DevicePin { get; set; } = string.Empty;
    public DateTime PunchTime { get; set; }
    public string PunchKind { get; set; } = string.Empty;
    public string StatusApplied { get; set; } = string.Empty;
    public Guid? StudentId { get; set; }
    public string? StudentName { get; set; }
    public Guid? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public Guid? ExamId { get; set; }
    public Guid? SubjectId { get; set; }
    public string? RawLine { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PunchLogListResponseDto
{
    public IReadOnlyList<PunchLogItemDto> Data { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ManualPunchDto
{
    public string SerialNumber { get; set; } = string.Empty;
    public string DevicePin { get; set; } = string.Empty;
    public DateTime? PunchTime { get; set; }
}
