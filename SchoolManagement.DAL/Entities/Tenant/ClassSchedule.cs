namespace SchoolManagement.DAL.Entities.Tenant;

public class ClassSchedule
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public string Day { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ClassEntity Class { get; set; } = null!;
    public Section Section { get; set; } = null!;
    public ICollection<ClassSchedulePeriod> Periods { get; set; } = new List<ClassSchedulePeriod>();
}

public class ClassSchedulePeriod
{
    public Guid Id { get; set; }
    public Guid ScheduleId { get; set; }
    public bool IsBreak { get; set; }
    public Guid? SubjectId { get; set; }
    public Guid? EmployeeId { get; set; }
    public TimeSpan StartingTime { get; set; }
    public TimeSpan EndingTime { get; set; }
    public string? ClassRoom { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ClassSchedule Schedule { get; set; } = null!;
    public Subject? Subject { get; set; }
    public Employee? Employee { get; set; }
}

public static class WeekDays
{
    public static readonly string[] All =
        ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
}
