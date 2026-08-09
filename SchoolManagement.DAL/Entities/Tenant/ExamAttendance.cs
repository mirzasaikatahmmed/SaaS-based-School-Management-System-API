namespace SchoolManagement.DAL.Entities.Tenant;

public class ExamAttendance
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public Guid SubjectId { get; set; }
    public Guid StudentId { get; set; }
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public string Status { get; set; } = "Present";
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Exam Exam { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
    public Student Student { get; set; } = null!;
    public ClassEntity Class { get; set; } = null!;
    public Section Section { get; set; } = null!;
}
