namespace SchoolManagement.BLL.DTOs.StudentAccounting;

public class FeesGroupItemDto
{
    public Guid FeesTypeId { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public int SortOrder { get; set; }
}

public class CreateFeesGroupDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<FeesGroupItemDto> Items { get; set; } = [];
}

public class UpdateFeesGroupDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public List<FeesGroupItemDto> Items { get; set; } = [];
}

public class FeesGroupItemResponseDto
{
    public Guid Id { get; set; }
    public Guid FeesTypeId { get; set; }
    public string FeesTypeName { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public int SortOrder { get; set; }
}

public class FeesGroupResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TotalAmount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<FeesGroupItemResponseDto> Items { get; set; } = [];
}

public class FeesGroupLookupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}
