namespace SchoolManagement.BLL.DTOs.StudentAccounting;

public class CreateFeesTypeDto
{
    public string Name { get; set; } = string.Empty;
    public string? FeeCode { get; set; }
    public string? Description { get; set; }
}

public class UpdateFeesTypeDto
{
    public string Name { get; set; } = string.Empty;
    public string? FeeCode { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}

public class FeesTypeResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FeeCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class FeesTypeLookupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FeeCode { get; set; } = string.Empty;
}
