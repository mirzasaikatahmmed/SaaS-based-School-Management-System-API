namespace SchoolManagement.DAL.Entities.Tenant;

public class ClassTeacherAllocation
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public Guid EmployeeId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ClassEntity Class { get; set; } = null!;
    public Section Section { get; set; } = null!;
    public Employee Employee { get; set; } = null!;
}
