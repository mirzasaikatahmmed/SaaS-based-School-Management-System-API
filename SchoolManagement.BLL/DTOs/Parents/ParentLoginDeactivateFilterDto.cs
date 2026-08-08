namespace SchoolManagement.BLL.DTOs.Parents;

public class ParentLoginDeactivateFilterDto
{
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Export { get; set; }
}
