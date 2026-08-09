namespace SchoolManagement.BLL.DTOs.Leave;

public class CreateLeaveCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int Days { get; set; }
}

public class UpdateLeaveCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int Days { get; set; }
    public bool? IsActive { get; set; }
}

public class LeaveCategoryResponseDto
{
    public Guid Id { get; set; }
    public int Sl { get; set; }
    public string Branch { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int Days { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LeaveCategoryLookupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Days { get; set; }
    public string Role { get; set; } = string.Empty;
}

public class CreateLeaveRequestDto
{
    public Guid LeaveCategoryId { get; set; }
    public DateTime DateOfStart { get; set; }
    public DateTime DateOfEnd { get; set; }
    public string? Reason { get; set; }
}

public class AdminCreateLeaveRequestDto
{
    public string Role { get; set; } = string.Empty;
    public Guid EmployeeId { get; set; }
    public Guid LeaveCategoryId { get; set; }
    public DateTime DateOfStart { get; set; }
    public DateTime DateOfEnd { get; set; }
    public string? Reason { get; set; }
    public string? Comments { get; set; }
}

public class LeaveListItemDto
{
    public Guid Id { get; set; }
    public int Sl { get; set; }
    public string? Role { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public string LeaveCategory { get; set; } = string.Empty;
    public DateTime DateOfStart { get; set; }
    public DateTime DateOfEnd { get; set; }
    public int Days { get; set; }
    public DateTime ApplyDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Comments { get; set; }
    public string? AttachmentUrl { get; set; }
}

public class LeaveFilterDto
{
    public string? Status { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string? Export { get; set; }
}

public class LeaveManageFilterDto
{
    public string? Role { get; set; }
    public string? Status { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string? Export { get; set; }
}

public class ReviewLeaveDto
{
    public string? Comments { get; set; }
}

public class LeaveListResponseDto
{
    public IReadOnlyList<LeaveListItemDto> Data { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
