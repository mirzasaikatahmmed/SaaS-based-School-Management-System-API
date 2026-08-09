namespace SchoolManagement.BLL.DTOs.Marks;

public class ExamPositionFilterDto
{
    public Guid ExamId { get; set; }
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public int AcademicYear { get; set; }
}

public class ExamPositionItemDto
{
    public Guid? Id { get; set; }
    public int Sl { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string RegisterNo { get; set; } = string.Empty;
    public string? Roll { get; set; }
    public string? Category { get; set; }
    public decimal TotalMarks { get; set; }
    public decimal FullMarks { get; set; }
    public string TotalMarksDisplay { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
    public decimal Gpa { get; set; }
    public string Result { get; set; } = string.Empty;
    public int? Position { get; set; }
    public string? PrincipalComments { get; set; }
    public string? TeacherComments { get; set; }
}

public class SaveExamPositionItemDto
{
    public Guid StudentId { get; set; }
    public int? Position { get; set; }
    public string? PrincipalComments { get; set; }
    public string? TeacherComments { get; set; }
}

public class SaveExamPositionDto
{
    public Guid ExamId { get; set; }
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public int AcademicYear { get; set; }
    public List<SaveExamPositionItemDto> Items { get; set; } = [];
}
