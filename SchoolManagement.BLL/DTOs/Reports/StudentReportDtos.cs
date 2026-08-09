namespace SchoolManagement.BLL.DTOs.Reports;

public class StudentReportFilterDto
{
    public Guid? ClassId { get; set; }
    public Guid? SectionId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Search { get; set; }
    public string? Export { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class LoginCredentialRowDto
{
    public Guid StudentId { get; set; }
    public int Sl { get; set; }
    public string? PhotoUrl { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public string RegisterNo { get; set; } = string.Empty;
    public string? Roll { get; set; }
    public string? GuardianName { get; set; }
    public string StudentUsername { get; set; } = string.Empty;
    public string? StudentPassword { get; set; }
    public string? ParentUsername { get; set; }
    public string? ParentPassword { get; set; }
    public bool PasswordRevealAvailable { get; set; }
}

public class LoginCredentialReportDto
{
    public IReadOnlyList<LoginCredentialRowDto> Data { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public string Note { get; set; } =
        "Passwords are shown from the last value set by an admin (admission/reset). Accounts created before password-reveal storage show blank until reset.";
}

public class ResetStudentPasswordDto
{
    /// <summary>Optional. When empty, a random temporary password is generated.</summary>
    public string? NewPassword { get; set; }
    public bool ResetParentPassword { get; set; } = true;
    public string? NewParentPassword { get; set; }
}

public class ResetStudentPasswordResultDto
{
    public Guid StudentId { get; set; }
    public string StudentUsername { get; set; } = string.Empty;
    public string StudentPassword { get; set; } = string.Empty;
    public string? ParentUsername { get; set; }
    public string? ParentPassword { get; set; }
}

public class AdmissionReportRowDto
{
    public Guid StudentId { get; set; }
    public int Sl { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Gender { get; set; }
    public string RegisterNo { get; set; } = string.Empty;
    public string? Roll { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public string? GuardianName { get; set; }
    public DateTime AdmissionDate { get; set; }
}

public class AdmissionReportDto
{
    public string Summary { get; set; } = string.Empty;
    public IReadOnlyList<AdmissionReportRowDto> Data { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ClassSectionReportRowDto
{
    public int Sl { get; set; }
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public IReadOnlyList<ClassSectionCountDto> Sections { get; set; } = [];
    public int TotalStudents { get; set; }
}

public class ClassSectionCountDto
{
    public Guid SectionId { get; set; }
    public string SectionName { get; set; } = string.Empty;
    public int StudentCount { get; set; }
}

public class SiblingReportRowDto
{
    public int Sl { get; set; }
    public string GuardianName { get; set; } = string.Empty;
    public string? MobileNo { get; set; }
    public string? FatherName { get; set; }
    public string? MotherName { get; set; }
    public string? Occupation { get; set; }
    public IReadOnlyList<SiblingStudentDto> Siblings { get; set; } = [];
}

public class SiblingStudentDto
{
    public Guid StudentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RegisterNo { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string? Gender { get; set; }
}
