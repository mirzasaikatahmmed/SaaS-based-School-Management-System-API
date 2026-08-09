namespace SchoolManagement.DAL.Entities.Tenant;

public class Subject
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Author { get; set; }
    public string SubjectType { get; set; } = SubjectTypes.Theory;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ClassSubjectAssignmentItem> AssignmentItems { get; set; } = new List<ClassSubjectAssignmentItem>();
}

public static class SubjectTypes
{
    public const string Theory = "Theory";
    public const string Practical = "Practical";
    public const string Mandatory = "Mandatory";
    public const string Optional = "Optional";
    public static readonly string[] All = [Theory, Practical, Mandatory, Optional];
}
