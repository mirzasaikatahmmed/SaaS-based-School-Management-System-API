namespace SchoolManagement.BLL.DTOs.Academic;

public class CreateSectionDto
{
    public string Name { get; set; } = string.Empty;
    public int? Capacity { get; set; }
}

public class UpdateSectionDto
{
    public string Name { get; set; } = string.Empty;
    public int? Capacity { get; set; }
    public bool? IsActive { get; set; }
}

public class SectionResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? Capacity { get; set; }
    public bool IsActive { get; set; }
    public int StudentCount { get; set; }
    public int ClassLinkCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
