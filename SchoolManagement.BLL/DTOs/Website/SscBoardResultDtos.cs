namespace SchoolManagement.BLL.DTOs.Website;

public class SscBoardOptionDto
{
    public int BoardId { get; set; }
    public string BoardName { get; set; } = string.Empty;
}

public class SscBoardCaptchaDto
{
    public string ImageBase64 { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
    public long Timestamp { get; set; }
    /// <summary>When auto-solve succeeds, pre-filled captcha text; otherwise null.</summary>
    public string? CaptchaText { get; set; }
}

public class SscBoardSearchRequestDto
{
    public string RollNo { get; set; } = string.Empty;
    public string RegNo { get; set; } = string.Empty;
    /// <summary>Board id (e.g. 12) or board name (e.g. RAJSHAHI).</summary>
    public string Board { get; set; } = string.Empty;
    public int PassYear { get; set; }
    public string? CaptchaText { get; set; }
    public string? Hash { get; set; }
    public long? Timestamp { get; set; }
    /// <summary>When true and captcha is empty, server fetches captcha and attempts OCR (if configured).</summary>
    public bool AutoSolve { get; set; }
}

public class SscBoardSubjectDto
{
    public string SubjectCode { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;
    public string? Mark { get; set; }
}

public class SscBoardResultDto
{
    public string Roll { get; set; } = string.Empty;
    public string Registration { get; set; } = string.Empty;
    public string Board { get; set; } = string.Empty;
    public int BoardId { get; set; }
    public int Year { get; set; }
    public int? ApiStatus { get; set; }
    public string? ApiMessage { get; set; }
    public string? Name { get; set; }
    public string? FatherName { get; set; }
    public string? MotherName { get; set; }
    public string? Gpa { get; set; }
    public string? ResultStatus { get; set; }
    public string? GroupName { get; set; }
    public string? Session { get; set; }
    public string? StudentType { get; set; }
    public string? Institution { get; set; }
    public IReadOnlyList<SscBoardSubjectDto> Subjects { get; set; } = [];
    public string? RawResult { get; set; }
    /// <summary>Fresh captcha when captcha failed / auto-solve needs manual entry.</summary>
    public SscBoardCaptchaDto? Captcha { get; set; }
}
