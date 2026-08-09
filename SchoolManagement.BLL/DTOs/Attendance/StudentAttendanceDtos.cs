namespace SchoolManagement.BLL.DTOs.Attendance;

public class StudentAttendanceItemDto
{
    public Guid StudentId { get; set; }
    public string Status { get; set; } = "Present";
    public string? Remarks { get; set; }
}

public class SaveStudentAttendanceDto
{
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public List<StudentAttendanceItemDto> Items { get; set; } = [];
}

public class StudentAttendanceRowDto
{
    public Guid? Id { get; set; }
    public int Sl { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string RegisterNo { get; set; } = string.Empty;
    public string? Roll { get; set; }
    public string Status { get; set; } = "Present";
    public string? Remarks { get; set; }
}

public class StudentAttendanceForDateResponseDto
{
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public List<StudentAttendanceRowDto> Items { get; set; } = [];
}

public class StudentAttendanceReportFilterDto
{
    public Guid? ClassId { get; set; }
    public Guid? SectionId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}

public class StudentAttendanceReportRowDto
{
    public DateTime AttendanceDate { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string RegisterNo { get; set; } = string.Empty;
    public string? Roll { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Remarks { get; set; }
}

public class StudentAttendanceSummaryDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string RegisterNo { get; set; } = string.Empty;
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public int LateCount { get; set; }
    public int HalfDayCount { get; set; }
    public int TotalDays { get; set; }
}

public class StudentAttendanceReportResponseDto
{
    public List<StudentAttendanceReportRowDto> Rows { get; set; } = [];
    public List<StudentAttendanceSummaryDto> Summary { get; set; } = [];
}
