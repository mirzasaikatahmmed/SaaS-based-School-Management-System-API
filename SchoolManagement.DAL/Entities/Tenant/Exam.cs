namespace SchoolManagement.DAL.Entities.Tenant;

public class Exam
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? ExamTermId { get; set; }
    public string? ExamType { get; set; }
    public string? Remarks { get; set; }
    public bool IsPublished { get; set; }
    public bool IsResultPublished { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ExamTerm? ExamTerm { get; set; }
    public ICollection<ExamMarkDistribution> MarkDistributions { get; set; } = new List<ExamMarkDistribution>();
    public ICollection<ExamSchedule> Schedules { get; set; } = new List<ExamSchedule>();
    public ICollection<MarkEntry> MarkEntries { get; set; } = new List<MarkEntry>();
}
