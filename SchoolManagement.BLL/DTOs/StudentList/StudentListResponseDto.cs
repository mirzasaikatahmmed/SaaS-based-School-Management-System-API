namespace SchoolManagement.BLL.DTOs.StudentList;

public class StudentListResponseDto
{
    public List<StudentListItemDto> Data { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
