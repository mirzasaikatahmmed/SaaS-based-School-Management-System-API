namespace SchoolManagement.DAL.Entities.Tenant;

public class Section
{
    public Guid Id { get; set; }
    /// <summary>Legacy owner class; nullable when section is a global master linked via ClassSections.</summary>
    public Guid? ClassId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? Capacity { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ClassEntity? Class { get; set; }
    public ICollection<ClassSection> ClassSections { get; set; } = new List<ClassSection>();
    public ICollection<Student> Students { get; set; } = new List<Student>();
}
