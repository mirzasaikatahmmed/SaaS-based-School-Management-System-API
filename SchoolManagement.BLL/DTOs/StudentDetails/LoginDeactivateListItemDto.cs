namespace SchoolManagement.BLL.DTOs.StudentDetails;

public class LoginDeactivateListItemDto
{
    public Guid Id { get; set; }
    public bool IsSelected { get; set; } = false;
    public string? PhotoUrl { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RegisterNo { get; set; } = string.Empty;
    public string? Roll { get; set; }
    public string? GuardianName { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string? DeactivateReason { get; set; }
    public string? Email { get; set; }
    public string? MobileNo { get; set; }
    public bool IsLoginActive { get; set; }
}

public class LoginDeactivateListResponseDto
{
    public List<LoginDeactivateListItemDto> Data { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
