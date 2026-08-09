namespace SchoolManagement.BLL.DTOs.Employee;
public class EmployeeListResponseDto { public IReadOnlyList<EmployeeListItemDto> Data { get; set; } = []; public int TotalCount { get; set; } public int Page { get; set; } public int PageSize { get; set; } public int TotalPages { get; set; } }
