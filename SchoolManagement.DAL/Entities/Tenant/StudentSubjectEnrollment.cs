namespace SchoolManagement.DAL.Entities.Tenant;

/// <summary>
/// Elective enrollment: Higher Math OR Agriculture (mutually exclusive).
/// Biology is NOT an elective — it stays a normal class subject. The student then
/// declares <see cref="AdditionalSubjectId"/> for SSC GPA (elective OR Biology).
/// </summary>
public class StudentSubjectEnrollment
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    /// <summary>Chosen elective: Higher Math or Agriculture.</summary>
    public Guid SubjectId { get; set; }
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public int AcademicYear { get; set; }
    /// <summary>Mutually exclusive group key, e.g. "4th".</summary>
    public string ElectiveGroup { get; set; } = "4th";
    /// <summary>
    /// Declared Additional Subject for GPA (GP above 2).
    /// Must be either <see cref="SubjectId"/> (the elective) or a subject with CanBeAdditional (Biology).
    /// </summary>
    public Guid? AdditionalSubjectId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Student Student { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
    public Subject? AdditionalSubject { get; set; }
    public ClassEntity Class { get; set; } = null!;
    public Section Section { get; set; } = null!;
}

public static class ElectiveGroups
{
    public const string Fourth = "4th";
}

/// <summary>SSC / board GPA: bonus from additional subject is max(0, GP − 2).</summary>
public static class BdGpaRules
{
    public const decimal AdditionalSubjectBaseGp = 2m;
}
