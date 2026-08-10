using SchoolManagement.BLL.DTOs.Reports;
using SchoolManagement.BLL.DTOs.Website;

namespace SchoolManagement.BLL.Interfaces;

public interface IPublicWebsiteService
{
    Task<SiteContactDto> GetSettingsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<MenuItemDto>> GetMenuAsync(CancellationToken ct = default);
    Task<IReadOnlyList<FooterColumnDto>> GetFooterAsync(CancellationToken ct = default);
    Task<VisitorStatsDto> GetVisitorsAsync(CancellationToken ct = default);
    Task<VisitorStatsDto> HitVisitorsAsync(CancellationToken ct = default);
    Task<HomePageDto> GetHomeAsync(CancellationToken ct = default);
    Task<HistoryPageDto> GetHistoryAsync(CancellationToken ct = default);
    Task<PersonSpeechDto> GetSpeechAsync(string role, CancellationToken ct = default);
    Task<IReadOnlyList<TenurePersonDto>> GetPresidentsAsync(string? search, CancellationToken ct = default);
    Task<IReadOnlyList<TenurePersonDto>> GetHeadmastersAsync(string? search, CancellationToken ct = default);
    Task<CommitteeResponseDto> GetCommitteeAsync(CancellationToken ct = default);
    Task<IReadOnlyList<PublicStaffMemberDto>> GetTeachersAsync(string? search, CancellationToken ct = default);
    Task<IReadOnlyList<PublicStaffMemberDto>> GetOfficeStaffAsync(string? search, CancellationToken ct = default);
    Task<IReadOnlyList<NoticeItemDto>> GetNoticesAsync(int? limit, string? search, CancellationToken ct = default);
    Task<NoticeItemDto> GetNoticeAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<GalleryCategoryDto>> GetGalleryCategoriesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<GalleryItemDto>> GetGalleryAsync(Guid? categoryId, int? limit, CancellationToken ct = default);
    Task<GalleryItemDto> GetGalleryItemAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<DocumentItemDto>> GetDocumentsAsync(string? category, string? search, CancellationToken ct = default);
    Task<DocumentItemDto> GetDocumentAsync(Guid id, CancellationToken ct = default);
    Task<AcademicPageDto> GetAcademicPageAsync(string slug, CancellationToken ct = default);
    Task<AcademicPageDto> GetAcademicRoutineAsync(string type, CancellationToken ct = default);
    Task<IReadOnlyList<HandnoteItemDto>> GetHandnotesAsync(string? className, string? search, CancellationToken ct = default);
    Task<IReadOnlyList<OnlineClassGroupDto>> GetOnlineClassesAsync(string? className, CancellationToken ct = default);
    Task<ResultAnalyticsPageDto> GetResultAnalyticsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<PublishedResultItemDto>> GetPublishedResultsAsync(string? examType, CancellationToken ct = default);
    Task<IReadOnlyList<OnlineExamOptionDto>> GetOnlineResultExamsAsync(CancellationToken ct = default);
    Task<ReportCardDto> SearchOnlineResultAsync(string registerNo, Guid examId, CancellationToken ct = default);
    Task<StudentStatisticsDto> GetStudentStatisticsAsync(int? academicYear, CancellationToken ct = default);
    Task<PublicStudentListDto> GetPublicStudentsAsync(
        Guid? classId, Guid? sectionId, string? className, string? sectionName,
        string? search, int page, int pageSize, int? academicYear, CancellationToken ct = default);
    Task<ContactPageDto> GetContactAsync(CancellationToken ct = default);
    Task<ContactMessageResultDto> SubmitContactAsync(ContactMessagePayloadDto dto, CancellationToken ct = default);
}
