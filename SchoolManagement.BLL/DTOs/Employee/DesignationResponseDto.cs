namespace SchoolManagement.BLL.DTOs.Employee;
public class DesignationResponseDto { public Guid Id { get; set; } public string Branch { get; set; } = string.Empty; public string Name { get; set; } = string.Empty; public bool IsActive { get; set; } public DateTime CreatedAt { get; set; } }
