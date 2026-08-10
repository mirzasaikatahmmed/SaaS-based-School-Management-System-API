using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.API.Filters;
using SchoolManagement.BLL.DTOs.Reports;
using SchoolManagement.BLL.DTOs.Website;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/public")]
[AllowAnonymous]
[RequireTenant]
public class PublicWebsiteController(IPublicWebsiteService service) : ControllerBase
{
    [HttpGet("site/settings")]
    public async Task<IActionResult> Settings(CancellationToken ct)
        => Ok(ApiResponse<SiteContactDto>.Ok(await service.GetSettingsAsync(ct), "Site settings retrieved"));

    [HttpGet("site/menu")]
    public async Task<IActionResult> Menu(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<MenuItemDto>>.Ok(await service.GetMenuAsync(ct), "Menu retrieved"));

    [HttpGet("site/footer")]
    public async Task<IActionResult> Footer(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FooterColumnDto>>.Ok(await service.GetFooterAsync(ct), "Footer retrieved"));

    [HttpGet("site/visitors")]
    public async Task<IActionResult> Visitors(CancellationToken ct)
        => Ok(ApiResponse<VisitorStatsDto>.Ok(await service.GetVisitorsAsync(ct), "Visitor stats retrieved"));

    [HttpPost("site/visitors/hit")]
    public async Task<IActionResult> HitVisitors(CancellationToken ct)
        => Ok(ApiResponse<VisitorStatsDto>.Ok(await service.HitVisitorsAsync(ct), "Visit recorded"));

    [HttpGet("home")]
    public async Task<IActionResult> Home(CancellationToken ct)
        => Ok(ApiResponse<HomePageDto>.Ok(await service.GetHomeAsync(ct), "Home retrieved"));

    [HttpGet("about/history")]
    public async Task<IActionResult> History(CancellationToken ct)
        => Ok(ApiResponse<HistoryPageDto>.Ok(await service.GetHistoryAsync(ct), "History retrieved"));

    [HttpGet("about/speeches/{role}")]
    public async Task<IActionResult> Speech(string role, CancellationToken ct)
        => Ok(ApiResponse<PersonSpeechDto>.Ok(await service.GetSpeechAsync(role, ct), "Speech retrieved"));

    [HttpGet("leadership/presidents")]
    public async Task<IActionResult> Presidents([FromQuery] string? search, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<TenurePersonDto>>.Ok(await service.GetPresidentsAsync(search, ct), "Presidents retrieved"));

    [HttpGet("leadership/headmasters")]
    public async Task<IActionResult> Headmasters([FromQuery] string? search, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<TenurePersonDto>>.Ok(await service.GetHeadmastersAsync(search, ct), "Headmasters retrieved"));

    [HttpGet("leadership/committee")]
    public async Task<IActionResult> Committee(CancellationToken ct)
        => Ok(ApiResponse<CommitteeResponseDto>.Ok(await service.GetCommitteeAsync(ct), "Committee retrieved"));

    [HttpGet("staff/teachers")]
    public async Task<IActionResult> Teachers([FromQuery] string? search, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PublicStaffMemberDto>>.Ok(await service.GetTeachersAsync(search, ct), "Teachers retrieved"));

    [HttpGet("staff/office")]
    public async Task<IActionResult> OfficeStaff([FromQuery] string? search, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PublicStaffMemberDto>>.Ok(await service.GetOfficeStaffAsync(search, ct), "Office staff retrieved"));

    [HttpGet("notices")]
    public async Task<IActionResult> Notices([FromQuery] int? limit, [FromQuery] string? search, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<NoticeItemDto>>.Ok(await service.GetNoticesAsync(limit, search, ct), "Notices retrieved"));

    [HttpGet("notices/{id:guid}")]
    public async Task<IActionResult> Notice(Guid id, CancellationToken ct)
        => Ok(ApiResponse<NoticeItemDto>.Ok(await service.GetNoticeAsync(id, ct), "Notice retrieved"));

    [HttpGet("gallery/categories")]
    public async Task<IActionResult> GalleryCategories(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<GalleryCategoryDto>>.Ok(await service.GetGalleryCategoriesAsync(ct), "Categories retrieved"));

    [HttpGet("gallery")]
    public async Task<IActionResult> Gallery([FromQuery] Guid? categoryId, [FromQuery] int? limit, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<GalleryItemDto>>.Ok(await service.GetGalleryAsync(categoryId, limit, ct), "Gallery retrieved"));

    [HttpGet("gallery/{id:guid}")]
    public async Task<IActionResult> GalleryItem(Guid id, CancellationToken ct)
        => Ok(ApiResponse<GalleryItemDto>.Ok(await service.GetGalleryItemAsync(id, ct), "Gallery item retrieved"));

    [HttpGet("documents")]
    public async Task<IActionResult> Documents([FromQuery] string? category, [FromQuery] string? search, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<DocumentItemDto>>.Ok(await service.GetDocumentsAsync(category, search, ct), "Documents retrieved"));

    [HttpGet("documents/{id:guid}")]
    public async Task<IActionResult> Document(Guid id, CancellationToken ct)
        => Ok(ApiResponse<DocumentItemDto>.Ok(await service.GetDocumentAsync(id, ct), "Document retrieved"));

    // ── Academic (routines, pages, handnotes, online class) ─────────────

    [HttpGet("academic/pages/{slug}")]
    public async Task<IActionResult> AcademicPage(string slug, CancellationToken ct)
        => Ok(ApiResponse<AcademicPageDto>.Ok(await service.GetAcademicPageAsync(slug, ct), "Academic page retrieved"));

    [HttpGet("academic/routines/{type}")]
    public async Task<IActionResult> AcademicRoutine(string type, CancellationToken ct)
        => Ok(ApiResponse<AcademicPageDto>.Ok(await service.GetAcademicRoutineAsync(type, ct), "Routine retrieved"));

    [HttpGet("academic/handnotes")]
    public async Task<IActionResult> Handnotes([FromQuery] string? className, [FromQuery] string? search, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<HandnoteItemDto>>.Ok(await service.GetHandnotesAsync(className, search, ct), "Handnotes retrieved"));

    [HttpGet("academic/online-classes")]
    public async Task<IActionResult> OnlineClasses([FromQuery] string? className, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<OnlineClassGroupDto>>.Ok(await service.GetOnlineClassesAsync(className, ct), "Online classes retrieved"));

    [HttpGet("results/analytics")]
    public async Task<IActionResult> ResultAnalytics(CancellationToken ct)
        => Ok(ApiResponse<ResultAnalyticsPageDto>.Ok(await service.GetResultAnalyticsAsync(ct), "Result analytics retrieved"));

    [HttpGet("results/ssc")]
    public async Task<IActionResult> PublishedResults([FromQuery] string? examType, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PublishedResultItemDto>>.Ok(await service.GetPublishedResultsAsync(examType, ct), "Published results retrieved"));

    /// <summary>Exams available for online result search (IsResultPublished = true).</summary>
    [HttpGet("results/exams")]
    public async Task<IActionResult> OnlineResultExams(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<OnlineExamOptionDto>>.Ok(await service.GetOnlineResultExamsAsync(ct), "Published exams retrieved"));

    /// <summary>Search student result by register number + exam.</summary>
    [HttpGet("results/search")]
    public async Task<IActionResult> OnlineResultSearch(
        [FromQuery] string registerNo,
        [FromQuery] Guid examId,
        CancellationToken ct)
        => Ok(ApiResponse<ReportCardDto>.Ok(
            await service.SearchOnlineResultAsync(registerNo, examId, ct),
            "Result retrieved"));

    [HttpGet("students/statistics")]
    public async Task<IActionResult> StudentStatistics([FromQuery] int? academicYear, CancellationToken ct)
        => Ok(ApiResponse<StudentStatisticsDto>.Ok(await service.GetStudentStatisticsAsync(academicYear, ct), "Student statistics retrieved"));

    [HttpGet("students")]
    public async Task<IActionResult> PublicStudents(
        [FromQuery] Guid? classId,
        [FromQuery] Guid? sectionId,
        [FromQuery] string? className,
        [FromQuery] string? sectionName,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 60,
        [FromQuery] int? academicYear = null,
        CancellationToken ct = default)
        => Ok(ApiResponse<PublicStudentListDto>.Ok(
            await service.GetPublicStudentsAsync(classId, sectionId, className, sectionName, search, page, pageSize, academicYear, ct),
            "Students retrieved"));

    [HttpGet("contact")]
    public async Task<IActionResult> Contact(CancellationToken ct)
        => Ok(ApiResponse<ContactPageDto>.Ok(await service.GetContactAsync(ct), "Contact page retrieved"));

    [HttpPost("contact/messages")]
    public async Task<IActionResult> ContactMessage([FromBody] ContactMessagePayloadDto dto, CancellationToken ct)
        => Ok(ApiResponse<ContactMessageResultDto>.Ok(await service.SubmitContactAsync(dto, ct), "Message submitted"));
}
