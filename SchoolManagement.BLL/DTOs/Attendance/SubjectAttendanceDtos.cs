namespace SchoolManagement.BLL.DTOs.Attendance;

public class SaveSubjectAttendanceDto
{
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public Guid SubjectId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public List<StudentAttendanceItemDto> Items { get; set; } = [];
}

public class SubjectAttendanceForDateResponseDto
{
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public Guid SubjectId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public string? SubjectName { get; set; }
    public List<StudentAttendanceRowDto> Items { get; set; } = [];
}
