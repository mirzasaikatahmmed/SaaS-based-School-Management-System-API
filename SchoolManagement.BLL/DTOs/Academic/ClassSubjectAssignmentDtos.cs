namespace SchoolManagement.BLL.DTOs.Academic;

public class UpsertClassSubjectAssignmentDto
{
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public List<Guid> SubjectIds { get; set; } = [];
}

public class ClassSubjectAssignmentResponseDto
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public Guid SectionId { get; set; }
    public string SectionName { get; set; } = string.Empty;
    public IReadOnlyList<SubjectLookupDto> Subjects { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
