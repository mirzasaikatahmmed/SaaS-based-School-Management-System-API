namespace SchoolManagement.BLL.DTOs.Academic;

public class ClassSubjectItemInputDto
{
    public Guid SubjectId { get; set; }
    /// <summary>Mark as elective (4th subject pool).</summary>
    public bool IsElective { get; set; }
    /// <summary>Mutually exclusive group, e.g. "4th". Required when IsElective is true.</summary>
    public string? ElectiveGroup { get; set; }
}

public class UpsertClassSubjectAssignmentDto
{
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    /// <summary>Legacy: all subjects treated as mandatory.</summary>
    public List<Guid> SubjectIds { get; set; } = [];
    /// <summary>Preferred: per-subject elective flags.</summary>
    public List<ClassSubjectItemInputDto> Items { get; set; } = [];
}

public class AssignedSubjectDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsElective { get; set; }
    public string? ElectiveGroup { get; set; }
}

public class ClassSubjectAssignmentResponseDto
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public Guid SectionId { get; set; }
    public string SectionName { get; set; } = string.Empty;
    public IReadOnlyList<AssignedSubjectDto> Subjects { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AssignStudentElectiveDto
{
    public Guid StudentId { get; set; }
    /// <summary>Higher Math or Agriculture.</summary>
    public Guid SubjectId { get; set; }
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public int AcademicYear { get; set; }
    public string ElectiveGroup { get; set; } = "4th";
    /// <summary>
    /// Declared Additional Subject for GPA (GP above 2).
    /// Must be SubjectId (elective) OR Biology (CanBeAdditional). Defaults to Biology if omitted and available.
    /// </summary>
    public Guid? AdditionalSubjectId { get; set; }
}

public class BulkAssignStudentElectiveDto
{
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public int AcademicYear { get; set; }
    public string ElectiveGroup { get; set; } = "4th";
    public List<StudentElectiveChoiceDto> Choices { get; set; } = [];
}

public class StudentElectiveChoiceDto
{
    public Guid StudentId { get; set; }
    public Guid SubjectId { get; set; }
    public Guid? AdditionalSubjectId { get; set; }
}

public class StudentElectiveRowDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string RegisterNo { get; set; } = string.Empty;
    public string? Roll { get; set; }
    public Guid? SubjectId { get; set; }
    public string? SubjectName { get; set; }
    public string? SubjectCode { get; set; }
    public Guid? AdditionalSubjectId { get; set; }
    public string? AdditionalSubjectName { get; set; }
    public string ElectiveGroup { get; set; } = "4th";
    public bool IsAssigned { get; set; }
}

public class StudentElectiveListDto
{
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public int AcademicYear { get; set; }
    public string ElectiveGroup { get; set; } = "4th";
    public IReadOnlyList<AssignedSubjectDto> Options { get; set; } = [];
    /// <summary>Subjects that can be declared Additional (Biology + the student's elective).</summary>
    public IReadOnlyList<AssignedSubjectDto> AdditionalOptions { get; set; } = [];
    public IReadOnlyList<StudentElectiveRowDto> Students { get; set; } = [];
    public string Note { get; set; } =
        "Choose Higher Math OR Agriculture for class/exam. Biology is not an elective — everyone can take it. Then declare Additional Subject for GPA as either your elective OR Biology (GP above 2).";
}
