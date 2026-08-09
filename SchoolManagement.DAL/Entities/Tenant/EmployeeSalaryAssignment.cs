namespace SchoolManagement.DAL.Entities.Tenant;

public class EmployeeSalaryAssignment
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid TemplateId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public Guid? AssignedBy { get; set; }
    public bool IsActive { get; set; } = true;

    public Employee Employee { get; set; } = null!;
    public SalaryTemplate Template { get; set; } = null!;
    public User? AssignedByUser { get; set; }
}
