namespace SchoolManagement.DAL.Entities.Tenant;

public class Section
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ClassEntity Class { get; set; } = null!;
    public ICollection<Student> Students { get; set; } = new List<Student>();
}
