namespace SchoolManagement.BLL.DTOs.Attendance;

public class EmployeeAttendanceItemDto
{
    public Guid EmployeeId { get; set; }
    public string Status { get; set; } = "Present";
    public string? Remarks { get; set; }
}

public class SaveEmployeeAttendanceDto
{
    public string? Role { get; set; }
    public DateTime AttendanceDate { get; set; }
    public List<EmployeeAttendanceItemDto> Items { get; set; } = [];
}

public class EmployeeAttendanceRowDto
{
    public Guid? Id { get; set; }
    public int Sl { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string StaffId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = "Present";
    public string? Remarks { get; set; }
}

public class EmployeeAttendanceForDateResponseDto
{
    public string? Role { get; set; }
    public DateTime AttendanceDate { get; set; }
    public List<EmployeeAttendanceRowDto> Items { get; set; } = [];
}

public class EmployeeAttendanceReportFilterDto
{
    public string? Role { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}

public class EmployeeAttendanceReportRowDto
{
    public DateTime AttendanceDate { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string StaffId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Remarks { get; set; }
}

public class EmployeeAttendanceSummaryDto
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string StaffId { get; set; } = string.Empty;
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public int LateCount { get; set; }
    public int HalfDayCount { get; set; }
    public int TotalDays { get; set; }
}

public class EmployeeAttendanceReportResponseDto
{
    public List<EmployeeAttendanceReportRowDto> Rows { get; set; } = [];
    public List<EmployeeAttendanceSummaryDto> Summary { get; set; } = [];
}
