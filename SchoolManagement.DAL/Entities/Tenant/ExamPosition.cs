namespace SchoolManagement.DAL.Entities.Tenant;

public class ExamPosition
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public Guid StudentId { get; set; }
    public int AcademicYear { get; set; }
    public decimal TotalMarks { get; set; }
    public decimal FullMarks { get; set; }
    public decimal Percentage { get; set; }
    public decimal Gpa { get; set; }
    public string Result { get; set; } = "FAIL";
    public int? Position { get; set; }
    public string? PrincipalComments { get; set; }
    public string? TeacherComments { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Exam Exam { get; set; } = null!;
    public ClassEntity Class { get; set; } = null!;
    public Section Section { get; set; } = null!;
    public Student Student { get; set; } = null!;
}
