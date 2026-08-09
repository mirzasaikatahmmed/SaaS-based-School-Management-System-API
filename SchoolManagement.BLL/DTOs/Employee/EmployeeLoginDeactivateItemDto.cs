namespace SchoolManagement.BLL.DTOs.Employee;

public class EmployeeLoginDeactivateItemDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public bool IsSelected { get; set; }
    public string? PhotoUrl { get; set; }
    public string Branch { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public string? Department { get; set; }
    public string? Email { get; set; }
    public string? MobileNo { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsLoginActive { get; set; }
}
