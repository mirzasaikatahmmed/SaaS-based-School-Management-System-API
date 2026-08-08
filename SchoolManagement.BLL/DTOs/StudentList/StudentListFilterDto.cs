namespace SchoolManagement.BLL.DTOs.StudentList;

public class StudentListFilterDto
{
    public Guid? ClassId { get; set; }
    public Guid? SectionId { get; set; }
    public int? AcademicYear { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public string? SortDir { get; set; }
    public string? Export { get; set; }
}
