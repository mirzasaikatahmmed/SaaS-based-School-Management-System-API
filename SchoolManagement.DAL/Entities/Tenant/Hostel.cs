namespace SchoolManagement.DAL.Entities.Tenant;

public class Hostel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<HostelRoom> Rooms { get; set; } = new List<HostelRoom>();
    public ICollection<Student> Students { get; set; } = new List<Student>();
}
