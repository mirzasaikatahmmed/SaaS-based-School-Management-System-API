using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IWebsiteRepository
{
    Task<WebsiteCmsSettings?> GetCmsSettingsAsync(CancellationToken ct = default);
    Task EnsureCmsSettingsRowAsync(CancellationToken ct = default);
    Task<IReadOnlyList<WebsiteMenuItem>> GetMenuAsync(CancellationToken ct = default);
    Task<IReadOnlyList<WebsiteFooterLink>> GetFooterLinksAsync(CancellationToken ct = default);
    Task<IReadOnlyList<WebsiteSliderItem>> GetSlidersAsync(CancellationToken ct = default);
    Task<IReadOnlyList<WebsiteImportantLink>> GetImportantLinksAsync(CancellationToken ct = default);
    Task<WebsiteSpeech?> GetSpeechAsync(string role, CancellationToken ct = default);
    Task<IReadOnlyList<WebsiteTenurePerson>> GetTenureAsync(string kind, string? search, CancellationToken ct = default);
    Task<IReadOnlyList<WebsiteCommitteeMember>> GetCommitteeAsync(CancellationToken ct = default);
    Task<IReadOnlyList<WebsiteNotice>> GetNoticesAsync(int? limit, string? search, CancellationToken ct = default);
    Task<WebsiteNotice?> GetNoticeAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<WebsiteGalleryCategory>> GetGalleryCategoriesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<WebsiteGalleryItem>> GetGalleryAsync(Guid? categoryId, int? limit, CancellationToken ct = default);
    Task<WebsiteGalleryItem?> GetGalleryItemAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<WebsiteDocument>> GetDocumentsAsync(string? category, string? search, CancellationToken ct = default);
    Task<WebsiteDocument?> GetDocumentAsync(Guid id, CancellationToken ct = default);
    Task<WebsiteContentPage?> GetContentPageAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<WebsiteHandnote>> GetHandnotesAsync(string? className, string? search, CancellationToken ct = default);
    Task<IReadOnlyList<WebsiteOnlineClassVideo>> GetOnlineClassVideosAsync(string? className, CancellationToken ct = default);
    Task<IReadOnlyList<WebsiteResultAnalyticsRow>> GetResultAnalyticsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<WebsitePublishedResult>> GetPublishedResultsAsync(string? examType, CancellationToken ct = default);
    Task<(int Today, int Last7, int Total)> GetVisitorStatsAsync(CancellationToken ct = default);
    Task<(int Today, int Last7, int Total)> HitVisitorAsync(CancellationToken ct = default);
    Task AddContactMessageAsync(WebsiteContactMessage message, CancellationToken ct = default);
}
