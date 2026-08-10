namespace SchoolManagement.BLL.DTOs.Website;

public class SiteContactDto
{
    public string SchoolName { get; set; } = string.Empty;
    public string? SchoolNameBn { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? FaviconUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public string? YoutubeUrl { get; set; }
    public string? PortalUrl { get; set; }
    public string? CopyrightText { get; set; }
}

public class MenuItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? TitleBn { get; set; }
    public string Path { get; set; } = "/";
    public bool OpenInNewTab { get; set; }
    public IReadOnlyList<MenuItemDto> Children { get; set; } = [];
}

public class FooterColumnDto
{
    public string Title { get; set; } = string.Empty;
    public string? TitleBn { get; set; }
    public IReadOnlyList<FooterLinkDto> Links { get; set; } = [];
}

public class FooterLinkDto
{
    public string Label { get; set; } = string.Empty;
    public string? LabelBn { get; set; }
    public string Path { get; set; } = "/";
    public bool External { get; set; }
}

public class VisitorStatsDto
{
    public int ViewsToday { get; set; }
    public int ViewsLast7Days { get; set; }
    public int TotalViews { get; set; }
    public DateTime ServerTime { get; set; }
}

public class PersonSpeechDto
{
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
}

public class SpeechPreviewDto
{
    public string Name { get; set; } = string.Empty;
    public string? NameBn { get; set; }
    public string Designation { get; set; } = string.Empty;
    public string? DesignationBn { get; set; }
    public string? PhotoUrl { get; set; }
    public string MessageHtml { get; set; } = string.Empty;
    public string ReadMorePath { get; set; } = string.Empty;
}

public class HistoryPageDto
{
    public string Title { get; set; } = "History";
    public string? TitleBn { get; set; }
    public HistoryProfileDto Profile { get; set; } = new();
    public IReadOnlyList<HistorySectionDto> Sections { get; set; } = [];
    public IReadOnlyList<FoundingCommitteeRowDto>? FoundingCommittee { get; set; }
}

public class HistoryProfileDto
{
    public string? Eiin { get; set; }
    public int? EstablishedYear { get; set; }
    public string? SchoolType { get; set; }
    public string? ClassesOffered { get; set; }
    public string? TotalStudentsLabel { get; set; }
    public string? Website { get; set; }
    public string? Address { get; set; }
    public string? ImageUrl { get; set; }
}

public class HistorySectionDto
{
    public string Heading { get; set; } = string.Empty;
    public string? HeadingBn { get; set; }
    public string BodyHtml { get; set; } = string.Empty;
}

public class FoundingCommitteeRowDto
{
    public int Sl { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
}

public class TenurePersonDto
{
    public Guid Id { get; set; }
    public int Sl { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public DateTime? JoinedOn { get; set; }
    public DateTime? LeftOn { get; set; }
}

public class CommitteeResponseDto
{
    public IReadOnlyList<CommitteeCategoryDto> Categories { get; set; } = [];
}

public class CommitteeCategoryDto
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? TitleBn { get; set; }
    public IReadOnlyList<CommitteeMemberDto> Members { get; set; } = [];
}

public class CommitteeMemberDto
{
    public Guid Id { get; set; }
    public int Sl { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? CategoryBn { get; set; }
    public string? PhotoUrl { get; set; }
    public string? MobileNo { get; set; }
}

public class PublicStaffMemberDto
{
    public Guid Id { get; set; }
    public int Sl { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? IndexNo { get; set; }
    public string Designation { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string? PhotoUrl { get; set; }
    public IReadOnlyList<string> MobileNos { get; set; } = [];
    public string? Email { get; set; }
    public IReadOnlyList<StaffQualificationDto> Qualifications { get; set; } = [];
    public DateTime? FirstJoiningDate { get; set; }
    public DateTime? MpoDate { get; set; }
    public DateTime? PresentJoiningDate { get; set; }
    public DateTime? DateOfBirth { get; set; }
}

public class StaffQualificationDto
{
    public string Degree { get; set; } = string.Empty;
    public string? Result { get; set; }
    public int? Year { get; set; }
}

public class NoticeItemDto
{
    public Guid Id { get; set; }
    public int Sl { get; set; }
    public DateTime PublishedOn { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? ViewUrl { get; set; }
    public string? FileUrl { get; set; }
    public string? BodyHtml { get; set; }
}

public class GalleryCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class GalleryItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ThumbUrl { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public DateTime? Date { get; set; }
    public string? Description { get; set; }
    public IReadOnlyList<string>? Images { get; set; }
}

public class DocumentItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? TitleBn { get; set; }
    public string? Category { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public DateTime? PublishedOn { get; set; }
}

public class ImportantLinkDto
{
    public Guid Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class SliderItemDto
{
    public string ImageUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public string? ButtonText { get; set; }
    public string? ButtonUrl { get; set; }
}

public class HomePageDto
{
    public IReadOnlyList<SliderItemDto> Slider { get; set; } = [];
    public SpeechPreviewDto? PresidentPreview { get; set; }
    public SpeechPreviewDto? HeadmasterPreview { get; set; }
    public IReadOnlyList<NoticeItemDto> Notices { get; set; } = [];
    public IReadOnlyList<GalleryItemDto> GalleryPreview { get; set; } = [];
    public IReadOnlyList<ImportantLinkDto> ImportantLinks { get; set; } = [];
    public VisitorStatsDto VisitorStats { get; set; } = new();
    public string? FacebookPageUrl { get; set; }
    public bool OnlineAdmissionEnabled { get; set; }
}

public class ContactPageDto
{
    public string PageTitle { get; set; } = "Contact Us";
    public string? BoxTitle { get; set; }
    public string? BoxDescription { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? MapIframeHtml { get; set; }
    public string? SubmitButtonText { get; set; }
}

public class ContactMessagePayloadDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Subject { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ContactMessageResultDto
{
    public Guid Id { get; set; }
}

public class AcademicPageDto
{
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? TitleBn { get; set; }
    public string? BodyHtml { get; set; }
    /// <summary>Optional PDF/file for embed viewer (routines, prospectus, etc.).</summary>
    public string? FileUrl { get; set; }
    public IReadOnlyList<DocumentItemDto> Documents { get; set; } = [];
}

public class HandnoteItemDto
{
    public Guid Id { get; set; }
    public int Sl { get; set; }
    public DateTime PublishedOn { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? TeacherName { get; set; }
    public string DownloadUrl { get; set; } = string.Empty;
}

public class OnlineClassVideoDto
{
    public Guid Id { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string? TeacherName { get; set; }
    public string YoutubeUrl { get; set; } = string.Empty;
    public string? YoutubeVideoId { get; set; }
    public string? EmbedUrl { get; set; }
    public DateTime? ClassDate { get; set; }
}

public class OnlineClassGroupDto
{
    public string ClassName { get; set; } = string.Empty;
    public IReadOnlyList<OnlineClassVideoDto> Videos { get; set; } = [];
}

public class ResultPassFailRowDto
{
    public int Year { get; set; }
    public int Appeared { get; set; }
    public int Passed { get; set; }
    public int NotPassed { get; set; }
    public decimal PassPercent { get; set; }
    public int Gpa5 { get; set; }
    public decimal Gpa5Percent { get; set; }
}

public class ResultGpaDistributionRowDto
{
    public int Year { get; set; }
    public int Gpa5 { get; set; }
    public int Gpa4x { get; set; }
    public int Gpa3x { get; set; }
    public int Gpa2x { get; set; }
    public int Gpa1x { get; set; }
}

public class ResultExamAnalyticsDto
{
    public string ExamType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public IReadOnlyList<ResultPassFailRowDto> PassFailStats { get; set; } = [];
    public IReadOnlyList<ResultGpaDistributionRowDto> GpaDistribution { get; set; } = [];
}

public class ResultAnalyticsPageDto
{
    public ResultExamAnalyticsDto SscExam { get; set; } = new() { ExamType = "SSC", Title = "SSC Exam" };
    public ResultExamAnalyticsDto SscVocational { get; set; } = new() { ExamType = "SSCVocational", Title = "SSC Vocational Exam" };
}

public class PublishedResultItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? TitleBn { get; set; }
    public string ExamType { get; set; } = string.Empty;
    public int Year { get; set; }
    public string? DetailUrl { get; set; }
    public string? FileUrl { get; set; }
}

/// <summary>Dropdown option for online result search (exams with IsResultPublished).</summary>
public class OnlineExamOptionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ExamType { get; set; }
    public string? TermName { get; set; }
}

/// <summary>Online result lookup request (register no + exam).</summary>
public class OnlineResultSearchRequestDto
{
    public string RegisterNo { get; set; } = string.Empty;
    public Guid ExamId { get; set; }
}

public class StudentStatRowDto
{
    public string ClassName { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public Guid? ClassId { get; set; }
    public Guid? SectionId { get; set; }
    public int Male { get; set; }
    public int Female { get; set; }
    public int Total { get; set; }
}

public class StudentStatisticsDto
{
    public IReadOnlyList<StudentStatRowDto> Rows { get; set; } = [];
    public int MaleTotal { get; set; }
    public int FemaleTotal { get; set; }
    public int GrandTotal { get; set; }
}

public class PublicStudentRowDto
{
    public Guid Id { get; set; }
    public int Sl { get; set; }
    public string? PhotoUrl { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ClassName { get; set; }
    public string? SectionName { get; set; }
    public string RegisterNo { get; set; } = string.Empty;
    public string? Roll { get; set; }
    public string? FatherName { get; set; }
    public string? MotherName { get; set; }
}

public class PublicStudentListDto
{
    public IReadOnlyList<PublicStudentRowDto> Students { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public string? ClassName { get; set; }
    public string? SectionName { get; set; }
}
