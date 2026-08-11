namespace SchoolManagement.BLL.DTOs.Employee;

public class EmployeeListItemDto
{
    public Guid Id { get; set; }
    public int Sl { get; set; }
    public string? PhotoUrl { get; set; }
    public string? SignatureUrl { get; set; }
    public string Branch { get; set; } = string.Empty;
    public string StaffId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public string? Department { get; set; }
    public string? Email { get; set; }
    public string? MobileNo { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsLoginActive { get; set; }
}
