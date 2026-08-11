namespace SchoolManagement.DAL.Entities.Tenant;

public class ExamScheduleSubject
{
    public Guid Id { get; set; }
    public Guid ScheduleId { get; set; }
    public Guid SubjectId { get; set; }
    public DateTime ExamDate { get; set; }
    public TimeSpan StartingTime { get; set; }
    public TimeSpan EndingTime { get; set; }
    public Guid? HallId { get; set; }
    public decimal? WrittenFullMark { get; set; }
    public decimal? WrittenPassMark { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ExamSchedule Schedule { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
    public ExamHall? Hall { get; set; }
}
