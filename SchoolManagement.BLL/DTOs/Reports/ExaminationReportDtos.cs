namespace SchoolManagement.BLL.DTOs.Reports;

public class ExamReportStudentFilterDto
{
    public Guid? ExamId { get; set; }
    public IReadOnlyList<Guid>? ExamIds { get; set; }
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public int AcademicYear { get; set; }
    public string? Search { get; set; }
}

public class ExamReportStudentRowDto
{
    public int Sl { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string RegisterNo { get; set; } = string.Empty;
    public string? Roll { get; set; }
    public string? MobileNo { get; set; }
    public string? Remarks { get; set; }
}

public class ExamReportStudentListDto
{
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public int AcademicYear { get; set; }
    public string? ClassName { get; set; }
    public string? SectionName { get; set; }
    public IReadOnlyList<ExamReportStudentRowDto> Students { get; set; } = [];
}

public class GenerateExamCardsRequestDto
{
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public int AcademicYear { get; set; }
    /// <summary>Single exam (report card) or first of multi.</summary>
    public Guid? ExamId { get; set; }
    /// <summary>Progress reports — one or more exams.</summary>
    public List<Guid> ExamIds { get; set; } = [];
    public List<Guid> StudentIds { get; set; } = [];
    public bool PrintAttendance { get; set; } = true;
    public bool PrintGradeScale { get; set; }
    public DateTime? PrintDate { get; set; }
}

public class GradeScaleItemDto
{
    public string GradeName { get; set; } = string.Empty;
    public decimal GradePoint { get; set; }
    public decimal MinPercentage { get; set; }
    public decimal MaxPercentage { get; set; }
    public string? Remarks { get; set; }
}

public class AttendanceSummaryDto
{
    public int WorkingDays { get; set; }
    public int DaysAttended { get; set; }
    public decimal AttendancePercentage { get; set; }
}

public class ReportCardSubjectRowDto
{
    public Guid SubjectId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public decimal? ObtainedMarks { get; set; }
    public decimal FullMarks { get; set; }
    public string MarksDisplay { get; set; } = string.Empty;
    public decimal? WrittenMark { get; set; }
    public decimal? McqMark { get; set; }
    public string? Grade { get; set; }
    public decimal? GradePoint { get; set; }
    public string? Remark { get; set; }
    public int? SubjectPosition { get; set; }
    public bool IsAbsent { get; set; }
}

public class ReportCardDto
{
    public Guid StudentId { get; set; }
    public Guid ExamId { get; set; }
    public int AcademicYear { get; set; }
    public string? SchoolName { get; set; }
    public string? SchoolAddress { get; set; }
    public string? SchoolPhone { get; set; }
    public string? SchoolEmail { get; set; }
    public string? SchoolWebsite { get; set; }
    public string? LogoUrl { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string RegisterNo { get; set; } = string.Empty;
    public string? Roll { get; set; }
    public string? ExamName { get; set; }
    public string? FatherName { get; set; }
    public string? MotherName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? ClassName { get; set; }
    public string? SectionName { get; set; }
    public string? PhotoUrl { get; set; }
    public IReadOnlyList<ReportCardSubjectRowDto> Subjects { get; set; } = [];
    public ReportCardSubjectRowDto? AdditionalSubject { get; set; }
    public IReadOnlyList<ReportCardSubjectRowDto> ContinuousAssessment { get; set; } = [];
    public decimal GrandTotalObtained { get; set; }
    public decimal GrandTotalFull { get; set; }
    public string GrandTotalDisplay { get; set; } = string.Empty;
    public string? GrandTotalInWords { get; set; }
    public decimal AveragePercentage { get; set; }
    /// <summary>GPA including additional-subject bonus (GP above 2).</summary>
    public decimal Gpa { get; set; }
    /// <summary>GPA of main subjects only (without additional).</summary>
    public decimal GpaWithoutAdditional { get; set; }
    /// <summary>max(0, additionalGP − 2) — "GP Above 2" on board transcript.</summary>
    public decimal AdditionalGpAbove2 { get; set; }
    public string? OverallGrade { get; set; }
    public string Result { get; set; } = "FAIL";
    public int? Position { get; set; }
    public AttendanceSummaryDto? Attendance { get; set; }
    public IReadOnlyList<GradeScaleItemDto>? GradeScale { get; set; }
    public DateTime PrintDate { get; set; }
    public string? PrintedBy { get; set; }
}

public class ReportCardBatchDto
{
    public IReadOnlyList<ReportCardDto> Cards { get; set; } = [];
}

public class TabulationSubjectColumnDto
{
    public Guid SubjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal FullMarks { get; set; }
}

public class TabulationRowDto
{
    public string? Position { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string RegisterNo { get; set; } = string.Empty;
    public string? Roll { get; set; }
    /// <summary>SubjectId → marks obtained (null if absent/missing).</summary>
    public IReadOnlyDictionary<string, decimal?> SubjectMarks { get; set; } = new Dictionary<string, decimal?>();
    public decimal TotalMarks { get; set; }
    public decimal Gpa { get; set; }
    public string Result { get; set; } = "FAIL";
}

public class TabulationSheetDto
{
    public Guid ExamId { get; set; }
    public string? ExamName { get; set; }
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public int AcademicYear { get; set; }
    public string? ClassName { get; set; }
    public string? SectionName { get; set; }
    public IReadOnlyList<TabulationSubjectColumnDto> Subjects { get; set; } = [];
    public IReadOnlyList<TabulationRowDto> Rows { get; set; } = [];
}
