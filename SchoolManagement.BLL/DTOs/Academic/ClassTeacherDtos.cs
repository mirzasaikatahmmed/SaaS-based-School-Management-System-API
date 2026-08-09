namespace SchoolManagement.BLL.DTOs.Academic;

public class UpsertClassTeacherDto
{
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public Guid EmployeeId { get; set; }
}

public class ClassTeacherResponseDto
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public Guid SectionId { get; set; }
    public string SectionName { get; set; } = string.Empty;
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
