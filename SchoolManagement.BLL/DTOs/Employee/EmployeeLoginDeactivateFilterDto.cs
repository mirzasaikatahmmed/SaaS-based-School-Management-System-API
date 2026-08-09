namespace SchoolManagement.BLL.DTOs.Employee;
public class EmployeeLoginDeactivateFilterDto { public string? Role { get; set; } public string? Search { get; set; } public string? SortBy { get; set; } public string? SortDir { get; set; } public int Page { get; set; } = 1; public int PageSize { get; set; } = 20; }
