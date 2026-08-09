using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class WebsiteRepository(TenantDbContext context) : IWebsiteRepository
{
    public Task<WebsiteCmsSettings?> GetCmsSettingsAsync(CancellationToken ct = default)
        => context.WebsiteCmsSettings.OrderBy(x => x.CreatedAt).FirstOrDefaultAsync(ct);

    public async Task EnsureCmsSettingsRowAsync(CancellationToken ct = default)
    {
        if (await context.WebsiteCmsSettings.AnyAsync(ct)) return;
        await context.WebsiteCmsSettings.AddAsync(new WebsiteCmsSettings { Id = Guid.NewGuid() }, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<WebsiteMenuItem>> GetMenuAsync(CancellationToken ct = default)
        => await context.WebsiteMenuItems
            .Where(x => x.IsPublished)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<WebsiteFooterLink>> GetFooterLinksAsync(CancellationToken ct = default)
        => await context.WebsiteFooterLinks
            .Where(x => x.IsPublished)
            .OrderBy(x => x.ColumnKey).ThenBy(x => x.SortOrder)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<WebsiteSliderItem>> GetSlidersAsync(CancellationToken ct = default)
        => await context.WebsiteSliderItems
            .Where(x => x.IsPublished)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<WebsiteImportantLink>> GetImportantLinksAsync(CancellationToken ct = default)
        => await context.WebsiteImportantLinks
            .Where(x => x.IsPublished)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(ct);

    public Task<WebsiteSpeech?> GetSpeechAsync(string role, CancellationToken ct = default)
        => context.WebsiteSpeeches
            .Where(x => x.IsPublished && x.Role == role)
            .OrderByDescending(x => x.UpdatedAt)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<WebsiteTenurePerson>> GetTenureAsync(string kind, string? search, CancellationToken ct = default)
    {
        var q = context.WebsiteTenurePeople.Where(x => x.IsPublished && x.Kind == kind);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            q = q.Where(x => x.Name.ToLower().Contains(s) || (x.Designation != null && x.Designation.ToLower().Contains(s)));
        }
        return await q.OrderBy(x => x.SortOrder).ThenBy(x => x.JoinedOn).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<WebsiteCommitteeMember>> GetCommitteeAsync(CancellationToken ct = default)
        => await context.WebsiteCommitteeMembers
            .Where(x => x.IsPublished)
            .OrderBy(x => x.Category).ThenBy(x => x.SortOrder)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<WebsiteNotice>> GetNoticesAsync(int? limit, string? search, CancellationToken ct = default)
    {
        var q = context.WebsiteNotices.Where(x => x.IsPublished);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            q = q.Where(x => x.Subject.ToLower().Contains(s));
        }
        q = q.OrderByDescending(x => x.PublishedOn).ThenByDescending(x => x.CreatedAt);
        if (limit is > 0) q = q.Take(limit.Value);
        return await q.ToListAsync(ct);
    }

    public Task<WebsiteNotice?> GetNoticeAsync(Guid id, CancellationToken ct = default)
        => context.WebsiteNotices.FirstOrDefaultAsync(x => x.Id == id && x.IsPublished, ct);

    public async Task<IReadOnlyList<WebsiteGalleryCategory>> GetGalleryCategoriesAsync(CancellationToken ct = default)
        => await context.WebsiteGalleryCategories.OrderBy(x => x.SortOrder).ThenBy(x => x.Name).ToListAsync(ct);

    public async Task<IReadOnlyList<WebsiteGalleryItem>> GetGalleryAsync(Guid? categoryId, int? limit, CancellationToken ct = default)
    {
        var q = context.WebsiteGalleryItems.Include(x => x.Category).Where(x => x.IsPublished);
        if (categoryId.HasValue) q = q.Where(x => x.CategoryId == categoryId);
        q = q.OrderBy(x => x.SortOrder).ThenByDescending(x => x.EventDate);
        if (limit is > 0) q = q.Take(limit.Value);
        return await q.ToListAsync(ct);
    }

    public Task<WebsiteGalleryItem?> GetGalleryItemAsync(Guid id, CancellationToken ct = default)
        => context.WebsiteGalleryItems.Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsPublished, ct);

    public async Task<IReadOnlyList<WebsiteDocument>> GetDocumentsAsync(string? category, string? search, CancellationToken ct = default)
    {
        var q = context.WebsiteDocuments.Where(x => x.IsPublished);
        if (!string.IsNullOrWhiteSpace(category))
            q = q.Where(x => x.Category == category.Trim());
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            q = q.Where(x => x.Title.ToLower().Contains(s) || (x.TitleBn != null && x.TitleBn.ToLower().Contains(s)));
        }
        return await q.OrderBy(x => x.SortOrder).ThenByDescending(x => x.PublishedOn).ToListAsync(ct);
    }

    public Task<WebsiteDocument?> GetDocumentAsync(Guid id, CancellationToken ct = default)
        => context.WebsiteDocuments.FirstOrDefaultAsync(x => x.Id == id && x.IsPublished, ct);

    public Task<WebsiteContentPage?> GetContentPageAsync(string slug, CancellationToken ct = default)
        => context.WebsiteContentPages.FirstOrDefaultAsync(
            x => x.IsPublished && x.Slug == slug.Trim().ToLowerInvariant(), ct);

    public async Task<IReadOnlyList<WebsiteHandnote>> GetHandnotesAsync(string? className, string? search, CancellationToken ct = default)
    {
        var q = context.WebsiteHandnotes.Where(x => x.IsPublished);
        if (!string.IsNullOrWhiteSpace(className))
            q = q.Where(x => x.ClassName.ToLower() == className.Trim().ToLower());
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            q = q.Where(x => x.Title.ToLower().Contains(s)
                             || (x.TeacherName != null && x.TeacherName.ToLower().Contains(s))
                             || x.ClassName.ToLower().Contains(s));
        }
        return await q.OrderByDescending(x => x.PublishedOn).ThenBy(x => x.SortOrder).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<WebsiteOnlineClassVideo>> GetOnlineClassVideosAsync(string? className, CancellationToken ct = default)
    {
        var q = context.WebsiteOnlineClassVideos.Where(x => x.IsPublished);
        if (!string.IsNullOrWhiteSpace(className))
            q = q.Where(x => x.ClassName.ToLower() == className.Trim().ToLower());
        return await q.OrderBy(x => x.ClassName).ThenBy(x => x.SortOrder).ThenByDescending(x => x.ClassDate).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<WebsiteResultAnalyticsRow>> GetResultAnalyticsAsync(CancellationToken ct = default)
        => await context.WebsiteResultAnalyticsRows
            .Where(x => x.IsPublished)
            .OrderByDescending(x => x.Year)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<WebsitePublishedResult>> GetPublishedResultsAsync(string? examType, CancellationToken ct = default)
    {
        var q = context.WebsitePublishedResults.Where(x => x.IsPublished);
        if (!string.IsNullOrWhiteSpace(examType))
            q = q.Where(x => x.ExamType == examType.Trim());
        return await q.OrderByDescending(x => x.Year).ThenBy(x => x.SortOrder).ToListAsync(ct);
    }

    public async Task<(int Today, int Last7, int Total)> GetVisitorStatsAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var from7 = today.AddDays(-6);
        var rows = await context.WebsiteVisitorDailies
            .Where(x => x.VisitDate >= from7)
            .ToListAsync(ct);
        var todayCount = rows.FirstOrDefault(x => x.VisitDate == today)?.Views ?? 0;
        var last7 = rows.Sum(x => x.Views);
        var total = await context.WebsiteVisitorDailies.SumAsync(x => (int?)x.Views, ct) ?? 0;
        return (todayCount, last7, total);
    }

    public async Task<(int Today, int Last7, int Total)> HitVisitorAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var row = await context.WebsiteVisitorDailies.FirstOrDefaultAsync(x => x.VisitDate == today, ct);
        if (row is null)
        {
            row = new WebsiteVisitorDaily { Id = Guid.NewGuid(), VisitDate = today, Views = 1 };
            await context.WebsiteVisitorDailies.AddAsync(row, ct);
        }
        else
        {
            row.Views += 1;
            context.WebsiteVisitorDailies.Update(row);
        }
        await context.SaveChangesAsync(ct);
        return await GetVisitorStatsAsync(ct);
    }

    public async Task AddContactMessageAsync(WebsiteContactMessage message, CancellationToken ct = default)
        => await context.WebsiteContactMessages.AddAsync(message, ct);
}
