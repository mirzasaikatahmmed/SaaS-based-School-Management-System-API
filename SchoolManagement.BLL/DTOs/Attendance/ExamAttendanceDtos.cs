namespace SchoolManagement.BLL.DTOs.Attendance;

public class ExamAttendanceFilterDto
{
    public Guid ExamId { get; set; }
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public Guid SubjectId { get; set; }
}

public class ExamAttendanceItemDto
{
    public Guid StudentId { get; set; }
    public string Status { get; set; } = "Present";
    public string? Remarks { get; set; }
}

public class SaveExamAttendanceDto
{
    public Guid ExamId { get; set; }
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public Guid SubjectId { get; set; }
    public List<ExamAttendanceItemDto> Items { get; set; } = [];
}

public class ExamAttendanceRowDto
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

public class ExamAttendanceResponseDto
{
    public Guid ExamId { get; set; }
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public Guid SubjectId { get; set; }
    public List<ExamAttendanceRowDto> Items { get; set; } = [];
}
