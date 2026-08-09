namespace SchoolManagement.BLL.DTOs.Reports;

public class AttendanceLegendItemDto
{
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

public class DayColumnDto
{
    public int Day { get; set; }
    public string Key { get; set; } = string.Empty;
    public string DayName { get; set; } = string.Empty;
    public bool IsWeekend { get; set; }
    public bool IsHoliday { get; set; }
}

public class MonthlyDayCellDto
{
    public int Day { get; set; }
    public string Key { get; set; } = string.Empty;
    /// <summary>W | H | P | A | L | HD | null (no mark)</summary>
    public string? Code { get; set; }
}

public class MonthlyAttendancePersonRowDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? RegisterNo { get; set; }
    public string? Roll { get; set; }
    public IReadOnlyList<MonthlyDayCellDto> Days { get; set; } = [];
    public decimal Percentage { get; set; }
    public int WeekendCount { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public int LateCount { get; set; }
    public int HalfDayCount { get; set; }
}

public class MonthlyAttendanceGridDto
{
    public string Title { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public Guid? ClassId { get; set; }
    public Guid? SectionId { get; set; }
    public Guid? SubjectId { get; set; }
    public string? ClassName { get; set; }
    public string? SectionName { get; set; }
    public string? SubjectName { get; set; }
    public string? Role { get; set; }
    public IReadOnlyList<AttendanceLegendItemDto> Legend { get; set; } = [];
    public IReadOnlyList<DayColumnDto> DayColumns { get; set; } = [];
    public IReadOnlyList<MonthlyAttendancePersonRowDto> Rows { get; set; } = [];
}

public class StudentDailyClassReportRowDto
{
    public int Sl { get; set; }
    public Guid ClassId { get; set; }
    public Guid? SectionId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string? SectionName { get; set; }
    public int Present { get; set; }
    public int TotalPresent { get; set; }
    public int TotalAbsent { get; set; }
    public decimal PresentPercent { get; set; }
    public decimal AbsentPercent { get; set; }
}

public class StudentDailyClassReportDto
{
    public DateTime Date { get; set; }
    public IReadOnlyList<StudentDailyClassReportRowDto> Rows { get; set; } = [];
    public int TotalPresent { get; set; }
    public int TotalAbsent { get; set; }
    public decimal PresentPercent { get; set; }
    public decimal AbsentPercent { get; set; }
}

public class StudentOverviewReportRowDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string RegisterNo { get; set; } = string.Empty;
    public DateTime AdmissionDate { get; set; }
    public string? Category { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string? Gender { get; set; }
    public string? MobileNo { get; set; }
    public int Count { get; set; }
}

public class StudentOverviewReportDto
{
    public string AttendanceType { get; set; } = "Present";
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public IReadOnlyList<StudentOverviewReportRowDto> Rows { get; set; } = [];
}

public class SubjectWiseDayReportRowDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string RegisterNo { get; set; } = string.Empty;
    public string? Roll { get; set; }
    public IReadOnlyDictionary<string, string?> SubjectStatuses { get; set; } = new Dictionary<string, string?>();
}

public class SubjectWiseDayReportDto
{
    public DateTime Date { get; set; }
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public string? ClassName { get; set; }
    public string? SectionName { get; set; }
    public IReadOnlyList<AttendanceLegendItemDto> Legend { get; set; } = [];
    public IReadOnlyList<SubjectColumnDto> Subjects { get; set; } = [];
    public IReadOnlyList<SubjectWiseDayReportRowDto> Rows { get; set; } = [];
}

public class SubjectColumnDto
{
    public Guid SubjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
}

public class SubjectWiseByDateRowDto
{
    public int Sl { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string RegisterNo { get; set; } = string.Empty;
    public string? Roll { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Remarks { get; set; }
}

public class SubjectWiseByDateReportDto
{
    public DateTime Date { get; set; }
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public Guid SubjectId { get; set; }
    public string? ClassName { get; set; }
    public string? SectionName { get; set; }
    public string? SubjectName { get; set; }
    public IReadOnlyList<AttendanceLegendItemDto> Legend { get; set; } = [];
    public IReadOnlyList<SubjectWiseByDateRowDto> Rows { get; set; } = [];
}

public class ExamAttendanceReportRowDto
{
    public int Sl { get; set; }
    public Guid StudentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RegisterNo { get; set; } = string.Empty;
    public string? Roll { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ExamAttendanceReportDto
{
    public Guid ExamId { get; set; }
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public Guid SubjectId { get; set; }
    public string? ExamName { get; set; }
    public string? ClassName { get; set; }
    public string? SectionName { get; set; }
    public string? SubjectName { get; set; }
    public IReadOnlyList<ExamAttendanceReportRowDto> Rows { get; set; } = [];
}

/// <summary>
/// Fingerprint / biometric punch log report — one row per punch (portal Attendance Report List).
/// </summary>
public class FingerprintLogFilterDto
{
    /// <summary>Student | Teacher | Accountant | …</summary>
    public string? Role { get; set; }
    public Guid? ClassId { get; set; }
    public Guid? SectionId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public Guid? DeviceId { get; set; }
    public string? Kind { get; set; }
    public Guid? StudentId { get; set; }
    public Guid? EmployeeId { get; set; }
    public string? DevicePin { get; set; }
    public string? Search { get; set; }
    public string? Export { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 100;
}

public class FingerprintLogRowDto
{
    public int Sl { get; set; }
    public Guid Id { get; set; }
    public string? PhotoUrl { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Roll { get; set; }
    /// <summary>Student register / admission number, or employee staff id.</summary>
    public string? AdmissionNo { get; set; }
    /// <summary>Biometric device PIN (Register ID on the terminal).</summary>
    public string RegisterId { get; set; } = string.Empty;
    /// <summary>Exact punch datetime from the device.</summary>
    public DateTime PunchTime { get; set; }
    public string PunchTimeIso { get; set; } = string.Empty;
    public string PunchDate { get; set; } = string.Empty;
    public string PunchClock { get; set; } = string.Empty;
    /// <summary>Device serial / terminal identifier.</summary>
    public string TerminalId { get; set; } = string.Empty;
    public string? DeviceName { get; set; }
    public DateTime ReceivedAt { get; set; }
    public string ReceivedAtIso { get; set; } = string.Empty;
    public string Role { get; set; } = "Unmapped";
    public string? ClassName { get; set; }
    public string? SectionName { get; set; }
    public Guid? StudentId { get; set; }
    public Guid? EmployeeId { get; set; }
    public Guid? DeviceId { get; set; }
    public string PunchKind { get; set; } = string.Empty;
    public string StatusApplied { get; set; } = string.Empty;
    public string? RawLine { get; set; }
}

public class FingerprintLogReportDto
{
    public string Title { get; set; } = "Attendance Report List";
    public string? Role { get; set; }
    public Guid? ClassId { get; set; }
    public Guid? SectionId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string Note { get; set; } =
        "Every fingerprint punch is one row with exact Punch Time and Terminal ID. Punches are not collapsed by day.";
    public IReadOnlyList<FingerprintLogRowDto> Rows { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
