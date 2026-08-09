namespace SchoolManagement.BLL.DTOs.Academic;

public class SectionLookupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? Capacity { get; set; }
}

public class CreateClassDto
{
    public string Name { get; set; } = string.Empty;
    public int? NumericName { get; set; }
    public List<Guid> SectionIds { get; set; } = [];
}

public class UpdateClassDto
{
    public string Name { get; set; } = string.Empty;
    public int? NumericName { get; set; }
    public bool? IsActive { get; set; }
    public List<Guid>? SectionIds { get; set; }
}

public class ClassResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? NumericName { get; set; }
    public bool IsActive { get; set; }
    public int StudentCount { get; set; }
    public IReadOnlyList<SectionLookupDto> Sections { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}
