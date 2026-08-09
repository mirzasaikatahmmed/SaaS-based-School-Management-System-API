namespace SchoolManagement.BLL.DTOs.ExamMaster;

public class CreateExamDto
{
    public string Name { get; set; } = string.Empty;
    public Guid? ExamTermId { get; set; }
    public string? ExamType { get; set; }
    public List<Guid> MarkDistributionIds { get; set; } = [];
    public string? Remarks { get; set; }
    public bool IsPublished { get; set; }
}

public class UpdateExamDto
{
    public string Name { get; set; } = string.Empty;
    public Guid? ExamTermId { get; set; }
    public string? ExamType { get; set; }
    public List<Guid> MarkDistributionIds { get; set; } = [];
    public string? Remarks { get; set; }
    public bool IsPublished { get; set; }
}

public class ExamListItemDto
{
    public Guid Id { get; set; }
    public int Sl { get; set; }
    public string Branch { get; set; } = string.Empty;
    public string ExamName { get; set; } = string.Empty;
    public string? ExamType { get; set; }
    public string? Term { get; set; }
    public List<string> MarkDistributions { get; set; } = [];
    public bool IsPublished { get; set; }
    public bool IsResultPublished { get; set; }
    public string? Remarks { get; set; }
}

public class ExamResponseDto
{
    public Guid Id { get; set; }
    public int Sl { get; set; }
    public string Branch { get; set; } = string.Empty;
    public string ExamName { get; set; } = string.Empty;
    public string? ExamType { get; set; }
    public string? Term { get; set; }
    public List<string> MarkDistributions { get; set; } = [];
    public bool IsPublished { get; set; }
    public bool IsResultPublished { get; set; }
    public string? Remarks { get; set; }
    public Guid? ExamTermId { get; set; }
    public List<Guid> MarkDistributionIds { get; set; } = [];
}

public class ExamLookupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? TermName { get; set; }
}
