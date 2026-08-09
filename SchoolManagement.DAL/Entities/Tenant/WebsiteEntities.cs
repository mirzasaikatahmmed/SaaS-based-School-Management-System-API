namespace SchoolManagement.DAL.Entities.Tenant;

public class WebsiteCmsSettings
{
    public Guid Id { get; set; }
    public string? SchoolNameBn { get; set; }
    public string? FacebookUrl { get; set; }
    public string? YoutubeUrl { get; set; }
    public string? FacebookPageUrl { get; set; }
    public string? PortalUrl { get; set; } = "/portal";
    public string? CopyrightText { get; set; }
    public bool OnlineAdmissionEnabled { get; set; } = true;
    public string? Eiin { get; set; }
    public int? EstablishedYear { get; set; }
    public string? SchoolType { get; set; }
    public string? ClassesOffered { get; set; }
    public string? TotalStudentsLabel { get; set; }
    public string? HistoryImageUrl { get; set; }
    public string? HistoryTitle { get; set; }
    public string? HistoryTitleBn { get; set; }
    /// <summary>JSON array of { heading, headingBn, bodyHtml }</summary>
    public string HistorySectionsJson { get; set; } = "[]";
    /// <summary>JSON array of { sl, name, designation }</summary>
    public string FoundingCommitteeJson { get; set; } = "[]";
    public string? ContactPageTitle { get; set; }
    public string? ContactBoxTitle { get; set; }
    public string? ContactBoxDescription { get; set; }
    public string? ContactMapIframeHtml { get; set; }
    public string? ContactSubmitButtonText { get; set; } = "Send";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class WebsiteMenuItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? TitleBn { get; set; }
    public string Path { get; set; } = "/";
    public Guid? ParentId { get; set; }
    public int SortOrder { get; set; }
    public bool OpenInNewTab { get; set; }
    public bool IsPublished { get; set; } = true;
    public WebsiteMenuItem? Parent { get; set; }
    public ICollection<WebsiteMenuItem> Children { get; set; } = new List<WebsiteMenuItem>();
}

public class WebsiteFooterLink
{
    public Guid Id { get; set; }
    public string ColumnKey { get; set; } = "institution"; // institution | student | academic | other
    public string ColumnTitle { get; set; } = string.Empty;
    public string? ColumnTitleBn { get; set; }
    public string Label { get; set; } = string.Empty;
    public string? LabelBn { get; set; }
    public string Path { get; set; } = "/";
    public bool IsExternal { get; set; }
    public int SortOrder { get; set; }
    public bool IsPublished { get; set; } = true;
}

public class WebsiteSliderItem
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public string? ButtonText { get; set; }
    public string? ButtonUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsPublished { get; set; } = true;
}

public class WebsiteImportantLink
{
    public Guid Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsPublished { get; set; } = true;
}

public class WebsiteSpeech
{
    public Guid Id { get; set; }
    /// <summary>President | Headmaster</summary>
    public string Role { get; set; } = WebsiteSpeechRoles.President;
    public string Title { get; set; } = string.Empty;
    public string? TitleBn { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameBn { get; set; }
    public string Designation { get; set; } = string.Empty;
    public string? DesignationBn { get; set; }
    public string? PhotoUrl { get; set; }
    public string MessageHtml { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? FacebookUrl { get; set; }
    public bool IsPublished { get; set; } = true;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public static class WebsiteSpeechRoles
{
    public const string President = "President";
    public const string Headmaster = "Headmaster";
}

public class WebsiteTenurePerson
{
    public Guid Id { get; set; }
    /// <summary>President | Headmaster</summary>
    public string Kind { get; set; } = WebsiteSpeechRoles.President;
    public string Name { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public DateTime? JoinedOn { get; set; }
    public DateTime? LeftOn { get; set; }
    public int SortOrder { get; set; }
    public bool IsPublished { get; set; } = true;
}

public class WebsiteCommitteeMember
{
    public Guid Id { get; set; }
    public string Category { get; set; } = WebsiteCommitteeCategories.President;
    public string? CategoryBn { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public string? MobileNo { get; set; }
    public int SortOrder { get; set; }
    public bool IsPublished { get; set; } = true;
}

public static class WebsiteCommitteeCategories
{
    public const string President = "President";
    public const string GuardianRepresentative = "GuardianRepresentative";
    public const string TeacherRepresentative = "TeacherRepresentative";
    public const string MemberSecretary = "MemberSecretary";
}

public class WebsiteNotice
{
    public Guid Id { get; set; }
    public DateTime PublishedOn { get; set; } = DateTime.UtcNow.Date;
    public string Subject { get; set; } = string.Empty;
    public string? BodyHtml { get; set; }
    public string? FileUrl { get; set; }
    public bool IsPublished { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class WebsiteGalleryCategory
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public ICollection<WebsiteGalleryItem> Items { get; set; } = new List<WebsiteGalleryItem>();
}

public class WebsiteGalleryItem
{
    public Guid Id { get; set; }
    public Guid? CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ThumbUrl { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    /// <summary>JSON array of extra image URLs</summary>
    public string ExtraImagesJson { get; set; } = "[]";
    public DateTime? EventDate { get; set; }
    public bool IsPublished { get; set; } = true;
    public int SortOrder { get; set; }
    public WebsiteGalleryCategory? Category { get; set; }
}

public class WebsiteDocument
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? TitleBn { get; set; }
    public string Category { get; set; } = "other";
    public string FileUrl { get; set; } = string.Empty;
    public DateTime? PublishedOn { get; set; }
    public bool IsPublished { get; set; } = true;
    public int SortOrder { get; set; }
}

/// <summary>Known document / academic page category slugs used by the public site.</summary>
public static class WebsiteDocumentCategories
{
    public const string ClassRoutine = "class-routine";
    public const string SchoolExamRoutine = "school-exam-routine";
    public const string SscExamRoutine = "ssc-exam-routine";
    public const string SscVocationalExamRoutine = "ssc-vocational-exam-routine";
    public const string Prospectus = "prospectus";
    public const string AdmissionProcess = "admission-process";
    public const string AdmissionTest = "admission-test";
    public const string AdmissionForm = "admission-form";
    public const string LessonPlanning = "lesson-planning";
    public const string Library = "library";
    public const string Laboratory = "laboratory";
    public const string Teaching = "teaching";
    public const string Recognition = "recognition";
    public const string Branch = "branch";
    public const string Mpo = "mpo";
    public const string Other = "other";

    public static readonly string[] AcademicPageSlugs =
    [
        Prospectus, AdmissionProcess, AdmissionTest, AdmissionForm,
        LessonPlanning, Library, Laboratory
    ];

    public static readonly string[] RoutineSlugs =
    [
        ClassRoutine, SchoolExamRoutine, SscExamRoutine, SscVocationalExamRoutine
    ];
}

/// <summary>HTML content page (library, laboratory, admission text, etc.) with optional PDF.</summary>
public class WebsiteContentPage
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? TitleBn { get; set; }
    public string? BodyHtml { get; set; }
    public string? FileUrl { get; set; }
    public bool IsPublished { get; set; } = true;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class WebsiteHandnote
{
    public Guid Id { get; set; }
    public DateTime PublishedOn { get; set; } = DateTime.UtcNow.Date;
    public string ClassName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? TeacherName { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public bool IsPublished { get; set; } = true;
    public int SortOrder { get; set; }
}

public class WebsiteOnlineClassVideo
{
    public Guid Id { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string? TeacherName { get; set; }
    public string YoutubeUrl { get; set; } = string.Empty;
    public string? YoutubeVideoId { get; set; }
    public DateTime? ClassDate { get; set; }
    public bool IsPublished { get; set; } = true;
    public int SortOrder { get; set; }
}

/// <summary>Yearly SSC / SSC Vocational pass & GPA analytics for the public site.</summary>
public class WebsiteResultAnalyticsRow
{
    public Guid Id { get; set; }
    /// <summary>SSC | SSCVocational</summary>
    public string ExamType { get; set; } = WebsiteExamTypes.Ssc;
    public int Year { get; set; }
    public int Appeared { get; set; }
    public int Passed { get; set; }
    public int NotPassed { get; set; }
    public decimal PassPercent { get; set; }
    public int Gpa5 { get; set; }
    public decimal Gpa5Percent { get; set; }
    public int Gpa4x { get; set; }
    public int Gpa3x { get; set; }
    public int Gpa2x { get; set; }
    public int Gpa1x { get; set; }
    public bool IsPublished { get; set; } = true;
}

public static class WebsiteExamTypes
{
    public const string Ssc = "SSC";
    public const string SscVocational = "SSCVocational";
}

/// <summary>Published board/result links (SSC 2024 Enter, etc.).</summary>
public class WebsitePublishedResult
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? TitleBn { get; set; }
    public string ExamType { get; set; } = WebsiteExamTypes.Ssc;
    public int Year { get; set; }
    public string? DetailUrl { get; set; }
    public string? FileUrl { get; set; }
    public bool IsPublished { get; set; } = true;
    public int SortOrder { get; set; }
}

public class WebsiteVisitorDaily
{
    public Guid Id { get; set; }
    public DateTime VisitDate { get; set; }
    public int Views { get; set; }
}

public class WebsiteContactMessage
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Subject { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; }
}
