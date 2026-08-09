namespace SchoolManagement.BLL.DTOs.ExamMaster;

public class CreateExamTermDto
{
    public string Name { get; set; } = string.Empty;
}

public class UpdateExamTermDto
{
    public string Name { get; set; } = string.Empty;
    public bool? IsActive { get; set; }
}

public class ExamTermResponseDto
{
    public Guid Id { get; set; }
    public int Sl { get; set; }
    public string Branch { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
