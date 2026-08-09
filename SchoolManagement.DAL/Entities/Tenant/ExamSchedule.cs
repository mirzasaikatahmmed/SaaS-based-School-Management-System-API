namespace SchoolManagement.DAL.Entities.Tenant;

public class ExamSchedule
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Exam Exam { get; set; } = null!;
    public ClassEntity Class { get; set; } = null!;
    public Section Section { get; set; } = null!;
    public ICollection<ExamScheduleSubject> Subjects { get; set; } = new List<ExamScheduleSubject>();
}
