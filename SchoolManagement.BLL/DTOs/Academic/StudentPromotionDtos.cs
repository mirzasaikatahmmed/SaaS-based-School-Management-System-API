namespace SchoolManagement.BLL.DTOs.Academic;

public class PromotionFilterDto
{
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public int? AcademicYear { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class PromotionStudentListItemDto
{
    public Guid Id { get; set; }
    public string RegisterNo { get; set; } = string.Empty;
    public string? Roll { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? ClassId { get; set; }
    public string? ClassName { get; set; }
    public Guid? SectionId { get; set; }
    public string? SectionName { get; set; }
    public int AcademicYear { get; set; }
}

public class PromotionStudentListResponseDto
{
    public IReadOnlyList<PromotionStudentListItemDto> Data { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ProcessPromotionItemDto
{
    public Guid StudentId { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? ToClassId { get; set; }
    public Guid? ToSectionId { get; set; }
    public int? ToAcademicYear { get; set; }
    public string? ToRoll { get; set; }
    public bool CarryForwardDue { get; set; } = true;
}

public class ProcessPromotionDto
{
    public List<ProcessPromotionItemDto> Items { get; set; } = [];
}

public class PromotionResultItemDto
{
    public Guid StudentId { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? Message { get; set; }
}

public class ProcessPromotionResultDto
{
    public int ProcessedCount { get; set; }
    public IReadOnlyList<PromotionResultItemDto> Results { get; set; } = [];
}
