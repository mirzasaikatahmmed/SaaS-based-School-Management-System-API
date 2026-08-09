namespace SchoolManagement.DAL.Entities.Tenant;

public class ClassSubjectAssignment
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ClassEntity Class { get; set; } = null!;
    public Section Section { get; set; } = null!;
    public ICollection<ClassSubjectAssignmentItem> Items { get; set; } = new List<ClassSubjectAssignmentItem>();
}

public class ClassSubjectAssignmentItem
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public Guid SubjectId { get; set; }

    public ClassSubjectAssignment Assignment { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
}
