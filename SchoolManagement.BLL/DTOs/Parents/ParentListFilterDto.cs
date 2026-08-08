namespace SchoolManagement.BLL.DTOs.Parents;

public class ParentListFilterDto
{
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public string? SortDir { get; set; }
    public string? Export { get; set; }
}
