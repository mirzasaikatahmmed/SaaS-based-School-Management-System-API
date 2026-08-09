namespace SchoolManagement.BLL.DTOs.Academic;

public class SchedulePeriodDto
{
    public bool IsBreak { get; set; }
    public Guid? SubjectId { get; set; }
    public Guid? EmployeeId { get; set; }
    public TimeSpan StartingTime { get; set; }
    public TimeSpan EndingTime { get; set; }
    public string? ClassRoom { get; set; }
    public int SortOrder { get; set; }
}

public class UpsertClassScheduleDto
{
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public string Day { get; set; } = string.Empty;
    public List<SchedulePeriodDto> Periods { get; set; } = [];
}

public class SchedulePeriodResponseDto
{
    public Guid Id { get; set; }
    public bool IsBreak { get; set; }
    public Guid? SubjectId { get; set; }
    public string? SubjectName { get; set; }
    public Guid? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public TimeSpan StartingTime { get; set; }
    public TimeSpan EndingTime { get; set; }
    public string? ClassRoom { get; set; }
    public int SortOrder { get; set; }
}

public class ClassScheduleResponseDto
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public Guid SectionId { get; set; }
    public string SectionName { get; set; } = string.Empty;
    public string Day { get; set; } = string.Empty;
    public IReadOnlyList<SchedulePeriodResponseDto> Periods { get; set; } = [];
}

public class TeacherSchedulePeriodDto
{
    public string ClassName { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public string? SubjectName { get; set; }
    public TimeSpan StartingTime { get; set; }
    public TimeSpan EndingTime { get; set; }
    public string? ClassRoom { get; set; }
    public bool IsBreak { get; set; }
}

public class TeacherScheduleDayDto
{
    public string Day { get; set; } = string.Empty;
    public IReadOnlyList<TeacherSchedulePeriodDto> Periods { get; set; } = [];
}
