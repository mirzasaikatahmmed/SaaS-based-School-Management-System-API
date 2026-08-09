namespace SchoolManagement.DAL.Entities.Tenant;

/// <summary>Many-to-many link so global sections can be assigned to classes.</summary>
public class ClassSection
{
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }

    public ClassEntity Class { get; set; } = null!;
    public Section Section { get; set; } = null!;
}
