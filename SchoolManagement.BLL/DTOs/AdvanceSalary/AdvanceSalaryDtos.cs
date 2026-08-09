namespace SchoolManagement.BLL.DTOs.AdvanceSalary;

public class CreateAdvanceSalaryDto
{
    public string Role { get; set; } = string.Empty;
    public Guid EmployeeId { get; set; }
    public string DeductMonth { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
}

public class CreateMyAdvanceSalaryDto
{
    public string DeductMonth { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
}

public class AdvanceSalaryListItemDto
{
    public Guid Id { get; set; }
    public int Sl { get; set; }
    public string? PhotoUrl { get; set; }
    public string Branch { get; set; } = string.Empty;
    public string ApplicantName { get; set; } = string.Empty;
    public string StaffId { get; set; } = string.Empty;
    public string StaffRole { get; set; } = string.Empty;
    public string DeductMonth { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime AppliedOn { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RejectReason { get; set; }
}

public class AdvanceSalaryMyListItemDto
{
    public Guid Id { get; set; }
    public int Sl { get; set; }
    public string? PhotoUrl { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public string DeductMonth { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime AppliedOn { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class AdvanceSalaryFilterDto
{
    public string? DeductMonth { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string? Export { get; set; }
}

public class AdvanceSalaryManageFilterDto
{
    public string? DeductMonth { get; set; }
    public string? Status { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string? Export { get; set; }
}

public class ReviewAdvanceSalaryDto
{
    public string? RejectReason { get; set; }
}

public class AdvanceSalaryResponseDto : AdvanceSalaryListItemDto
{
    public string? Reason { get; set; }
    public DateTime? ReviewedAt { get; set; }
}

public class AdvanceSalaryListResponseDto
{
    public IReadOnlyList<AdvanceSalaryListItemDto> Data { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class AdvanceSalaryMyListResponseDto
{
    public IReadOnlyList<AdvanceSalaryMyListItemDto> Data { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class HrEmployeeLookupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string StaffId { get; set; } = string.Empty;
}
