namespace SchoolManagement.BLL.DTOs.Parents;

public class ParentLoginDeactivateListItemDto
{
    public Guid Id { get; set; }
    public bool IsSelected { get; set; } = false;
    public string GuardianName { get; set; } = string.Empty;
    public string? Occupation { get; set; }
    public string? MobileNo { get; set; }
    public string? Email { get; set; }
    public bool IsLoginActive { get; set; }
}

public class ParentLoginDeactivateListResponseDto
{
    public List<ParentLoginDeactivateListItemDto> Data { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class BulkParentLoginActivateDto
{
    public List<Guid> ParentIds { get; set; } = new();
}

public class BulkParentLoginActivateResultDto
{
    public int Activated { get; set; }
    public int Failed { get; set; }
}
