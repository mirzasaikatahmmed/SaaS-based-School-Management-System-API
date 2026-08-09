namespace SchoolManagement.BLL.DTOs.Academic;

public class CreateSubjectDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Author { get; set; }
    public string SubjectType { get; set; } = string.Empty;
}

public class UpdateSubjectDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Author { get; set; }
    public string SubjectType { get; set; } = string.Empty;
    public bool? IsActive { get; set; }
}

public class SubjectResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Author { get; set; }
    public string SubjectType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SubjectLookupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
