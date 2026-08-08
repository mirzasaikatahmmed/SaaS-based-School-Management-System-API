namespace SchoolManagement.BLL.DTOs.StudentDetails;

public class LoginDeactivateFilterDto
{
    public Guid? ClassId { get; set; }
    public Guid? SectionId { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Export { get; set; }
}
