namespace SchoolManagement.BLL.DTOs.Employee;

public class EmployeeLoginDeactivateListResponseDto
{
    public IReadOnlyList<EmployeeLoginDeactivateItemDto> Data { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
