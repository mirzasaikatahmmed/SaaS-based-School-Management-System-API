namespace SchoolManagement.BLL.DTOs.ExamMaster;

public class MarkEntryFilterDto
{
    public Guid ExamId { get; set; }
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public Guid SubjectId { get; set; }
    public string? Export { get; set; }
}

public class MarkEntryStudentItemDto
{
    public Guid StudentId { get; set; }
    public Guid? MarkEntryId { get; set; }
    public int Sl { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string RegisterNo { get; set; } = string.Empty;
    public string? Roll { get; set; }
    public bool IsAbsent { get; set; }
    public decimal? WrittenMark { get; set; }
    public decimal? McqMark { get; set; }
    public decimal? WrittenFullMark { get; set; }
    public decimal? WrittenPassMark { get; set; }
}

public class MarkEntryListResponseDto
{
    public bool HasMcq { get; set; }
    public decimal? WrittenFullMark { get; set; }
    public decimal? WrittenPassMark { get; set; }
    public IReadOnlyList<MarkEntryStudentItemDto> Items { get; set; } = [];
}

public class SaveMarkEntriesDto
{
    public Guid ExamId { get; set; }
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public Guid SubjectId { get; set; }
    public List<StudentMarkDto> Marks { get; set; } = [];
}

public class StudentMarkDto
{
    public Guid StudentId { get; set; }
    public bool IsAbsent { get; set; }
    public decimal? WrittenMark { get; set; }
    public decimal? McqMark { get; set; }
}
