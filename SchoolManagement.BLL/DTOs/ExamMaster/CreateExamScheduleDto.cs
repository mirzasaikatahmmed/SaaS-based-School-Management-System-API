namespace SchoolManagement.BLL.DTOs.ExamMaster;

public class ExamScheduleFilterDto
{
    public Guid? ClassId { get; set; }
    public Guid? SectionId { get; set; }
}

public class CreateExamScheduleDto
{
    public Guid ExamId { get; set; }
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public DateTime? StartingDate { get; set; }
    public TimeSpan? StartingTime { get; set; }
    public int? ExamDurationMinutes { get; set; }
    public Guid? DefaultHallId { get; set; }
    public decimal? WrittenFullMark { get; set; }
    public decimal? WrittenPassMark { get; set; }
    public List<ExamScheduleSubjectDto> Subjects { get; set; } = [];
}

public class ExamScheduleSubjectDto
{
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public DateTime ExamDate { get; set; }
    public TimeSpan StartingTime { get; set; }
    public TimeSpan EndingTime { get; set; }
    public Guid? HallId { get; set; }
    public decimal? WrittenFullMark { get; set; }
    public decimal? WrittenPassMark { get; set; }
    public int SortOrder { get; set; }
}

public class ExamScheduleResponseDto
{
    public Guid Id { get; set; }
    public int Sl { get; set; }
    public string Branch { get; set; } = string.Empty;
    public string ExamName { get; set; } = string.Empty;
    public string? TermName { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public int SubjectCount { get; set; }
}

public class ExamScheduleDetailDto
{
    public Guid Id { get; set; }
    public string ExamName { get; set; } = string.Empty;
    public string TermName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public List<ExamScheduleSubjectDetailDto> Subjects { get; set; } = [];
}

public class ExamScheduleSubjectDetailDto
{
    public string SubjectName { get; set; } = string.Empty;
    public string ExamDate { get; set; } = string.Empty;
    public string StartingTime { get; set; } = string.Empty;
    public string EndingTime { get; set; } = string.Empty;
    public string? HallRoom { get; set; }
}
