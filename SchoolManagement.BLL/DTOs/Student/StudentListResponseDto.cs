namespace SchoolManagement.BLL.DTOs.Student;

public class StudentListResponseDto
{
    public IReadOnlyList<StudentResponseDto> Items { get; set; } = Array.Empty<StudentResponseDto>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}
