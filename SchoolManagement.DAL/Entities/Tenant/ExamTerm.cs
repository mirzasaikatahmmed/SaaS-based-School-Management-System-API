namespace SchoolManagement.DAL.Entities.Tenant;

public class ExamTerm
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Exam> Exams { get; set; } = new List<Exam>();
}
