namespace SchoolManagement.BLL.DTOs.StudentAccounting;

public class CreateFineSetupDto
{
    public Guid GroupId { get; set; }
    public Guid FeesTypeId { get; set; }
    public string FineType { get; set; } = string.Empty;
    public decimal FineValue { get; set; }
    public string? LateFeeFrequency { get; set; }
}

public class UpdateFineSetupDto
{
    public string FineType { get; set; } = string.Empty;
    public decimal FineValue { get; set; }
    public string? LateFeeFrequency { get; set; }
    public bool? IsActive { get; set; }
}

public class FineSetupResponseDto
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public Guid FeesTypeId { get; set; }
    public string FeesTypeName { get; set; } = string.Empty;
    public string FineType { get; set; } = string.Empty;
    public decimal FineValue { get; set; }
    public string? LateFeeFrequency { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
