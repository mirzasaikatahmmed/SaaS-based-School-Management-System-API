namespace SchoolManagement.BLL.DTOs.Biometric;

public class CreateUserMapDto
{
    public string DevicePin { get; set; } = string.Empty;
    public string PersonType { get; set; } = "Student"; // Student | Employee
    public Guid? StudentId { get; set; }
    public Guid? EmployeeId { get; set; }
}

public class UpdateUserMapDto
{
    public string? DevicePin { get; set; }
    public bool? IsActive { get; set; }
}

public class UserMapResponseDto
{
    public Guid Id { get; set; }
    public string DevicePin { get; set; } = string.Empty;
    public string PersonType { get; set; } = string.Empty;
    public Guid? StudentId { get; set; }
    public string? StudentName { get; set; }
    public string? StudentRegisterNo { get; set; }
    public Guid? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
