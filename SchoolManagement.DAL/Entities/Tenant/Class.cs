namespace SchoolManagement.DAL.Entities.Tenant;

public class ClassEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? NumericName { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Section> Sections { get; set; } = new List<Section>();
    public ICollection<ClassSection> ClassSections { get; set; } = new List<ClassSection>();
    public ICollection<Student> Students { get; set; } = new List<Student>();
}
