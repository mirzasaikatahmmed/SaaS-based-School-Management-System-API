using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.Events;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class EventService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IStorageService storage,
    IHttpContextAccessor http) : IEventService
{
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    public static readonly string[] Audiences = ["Everybody", "Student", "Teacher", "Employee", "Parent"];

    public async Task<EventListResponseDto> GetListAsync(EventFilterDto filter, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();

        var page = filter.Page < 1 ? 1 : filter.Page;
        var size = filter.PageSize is < 1 or > 200 ? 25 : filter.PageSize;
        var (items, total) = await uow.Events.SearchAsync(new EventSearchFilter
        {
            Search = filter.Search,
            EventTypeId = filter.EventTypeId,
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            Page = page,
            PageSize = size
        }, ct);

        var data = new List<EventListItemDto>();
        var i = 0;
        foreach (var e in items)
            data.Add(await MapList(e, (page - 1) * size + ++i, ct));

        return new EventListResponseDto
        {
            Data = data,
            TotalCount = total,
            Page = page,
            PageSize = size,
            TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)size)
        };
    }

    public async Task<EventDetailDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var entity = await uow.Events.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Event '{id}' not found.");
        return await MapDetail(entity, ct);
    }

    public async Task<EventDetailDto> CreateAsync(CreateEventDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();

        var title = dto.Title.Trim();
        ValidateDates(dto.DateOfStart, dto.DateOfEnd);
        var audience = ValidateAudience(dto.Audience);

        if (dto.EventTypeId.HasValue && await uow.EventTypes.GetByIdAsync(dto.EventTypeId.Value, ct) is null)
            throw new NotFoundException("Event type not found.");

        var entity = new SchoolEvent
        {
            Id = Guid.NewGuid(),
            Title = title,
            EventTypeId = dto.EventTypeId,
            IsHoliday = dto.IsHoliday,
            Audience = audience,
            DateOfStart = dto.DateOfStart.Date,
            DateOfEnd = dto.DateOfEnd.Date,
            Description = dto.Description?.Trim(),
            ShowWebsite = false,
            IsPublished = false,
            CreatedBy = TryCurrentUser(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await uow.Events.AddAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
        return await MapDetail(await uow.Events.GetByIdAsync(entity.Id, ct) ?? entity, ct);
    }

    public async Task<EventDetailDto> UpdateAsync(Guid id, UpdateEventDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();

        var entity = await uow.Events.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Event '{id}' not found.");

        ValidateDates(dto.DateOfStart, dto.DateOfEnd);
        var audience = ValidateAudience(dto.Audience);

        if (dto.EventTypeId.HasValue && await uow.EventTypes.GetByIdAsync(dto.EventTypeId.Value, ct) is null)
            throw new NotFoundException("Event type not found.");

        entity.Title = dto.Title.Trim();
        entity.EventTypeId = dto.EventTypeId;
        entity.IsHoliday = dto.IsHoliday;
        entity.Audience = audience;
        entity.DateOfStart = dto.DateOfStart.Date;
        entity.DateOfEnd = dto.DateOfEnd.Date;
        entity.Description = dto.Description?.Trim();
        if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;
        entity.UpdatedAt = DateTime.UtcNow;

        await uow.Events.UpdateAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
        return await MapDetail(await uow.Events.GetByIdAsync(id, ct) ?? entity, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var entity = await uow.Events.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Event '{id}' not found.");
        await uow.Events.DeleteAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
    }

    public async Task<EventDetailDto> UploadImageAsync(Guid id, Stream stream, string fileName, string contentType, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();

        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(ext) || !contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) ||
            (stream.CanSeek && stream.Length > 3 * 1024 * 1024))
            throw new AppException("Only jpg, jpeg, png, and webp images up to 3MB are allowed.", 400);

        var entity = await uow.Events.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Event '{id}' not found.");
        var slug = tenant.TenantSlug ?? throw new AppException("Tenant slug is not resolved.", 400);

        if (!string.IsNullOrWhiteSpace(entity.ImageUrl))
        {
            try { await storage.DeleteFileAsync(slug, entity.ImageUrl, ct); } catch { /* best effort */ }
        }

        var key = $"{AppConstants.StorageFolders.Events}/{id}{ext}";
        await storage.UploadObjectAsync(slug, key, stream, contentType, ct);
        entity.ImageUrl = key;
        entity.UpdatedAt = DateTime.UtcNow;

        await uow.Events.UpdateAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
        return await MapDetail(entity, ct);
    }

    public async Task<EventDetailDto> TogglePublishAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var entity = await uow.Events.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Event '{id}' not found.");
        entity.IsPublished = !entity.IsPublished;
        entity.UpdatedAt = DateTime.UtcNow;
        await uow.Events.UpdateAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
        return await MapDetail(entity, ct);
    }

    public async Task<EventDetailDto> ToggleShowWebsiteAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var entity = await uow.Events.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Event '{id}' not found.");
        entity.ShowWebsite = !entity.ShowWebsite;
        entity.UpdatedAt = DateTime.UtcNow;
        await uow.Events.UpdateAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
        return await MapDetail(entity, ct);
    }

    public async Task<IReadOnlyList<PublicEventDto>> GetPublicAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        var items = await uow.Events.GetPublicAsync(ct);
        var result = new List<PublicEventDto>();
        foreach (var e in items)
        {
            result.Add(new PublicEventDto
            {
                Id = e.Id,
                Title = e.Title,
                EventTypeName = e.EventType?.Name,
                IsHoliday = e.IsHoliday,
                Audience = e.Audience,
                DateOfStart = e.DateOfStart,
                DateOfEnd = e.DateOfEnd,
                Description = e.Description,
                ImageUrl = await Presign(e.ImageUrl, ct)
            });
        }
        return result;
    }

    private static void ValidateDates(DateTime start, DateTime end)
    {
        if (end.Date < start.Date)
            throw new AppException("DateOfEnd must be on or after DateOfStart.", 400);
    }

    private static string ValidateAudience(string audience)
    {
        var match = Audiences.FirstOrDefault(a => a.Equals(audience?.Trim(), StringComparison.OrdinalIgnoreCase));
        return match ?? throw new AppException("Invalid audience value.", 400);
    }

    private async Task<EventListItemDto> MapList(SchoolEvent e, int sl, CancellationToken ct) => new()
    {
        Id = e.Id,
        Sl = sl,
        Branch = tenant.TenantName ?? string.Empty,
        Title = e.Title,
        EventTypeName = e.EventType?.Name,
        IsHoliday = e.IsHoliday,
        Audience = e.Audience,
        DateOfStart = e.DateOfStart,
        DateOfEnd = e.DateOfEnd,
        ImageUrl = await Presign(e.ImageUrl, ct),
        ShowWebsite = e.ShowWebsite,
        IsPublished = e.IsPublished,
        IsActive = e.IsActive,
        CreatedByName = e.CreatedByUser is null ? null : $"{e.CreatedByUser.FirstName} {e.CreatedByUser.LastName}".Trim()
    };

    private async Task<EventDetailDto> MapDetail(SchoolEvent e, CancellationToken ct)
    {
        var list = await MapList(e, 0, ct);
        return new EventDetailDto
        {
            Id = list.Id,
            Sl = list.Sl,
            Branch = list.Branch,
            Title = list.Title,
            EventTypeName = list.EventTypeName,
            IsHoliday = list.IsHoliday,
            Audience = list.Audience,
            DateOfStart = list.DateOfStart,
            DateOfEnd = list.DateOfEnd,
            ImageUrl = list.ImageUrl,
            ShowWebsite = list.ShowWebsite,
            IsPublished = list.IsPublished,
            IsActive = list.IsActive,
            CreatedByName = list.CreatedByName,
            EventTypeId = e.EventTypeId,
            Description = e.Description,
            CreatedAt = e.CreatedAt
        };
    }

    private async Task<string?> Presign(string? key, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(tenant.TenantSlug)) return key;
        try { return await storage.GetPresignedUrlAsync(tenant.TenantSlug, key, ct); }
        catch { return key; }
    }

    private async Task Ready(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(tenant.SchemaName))
            throw new AppException("X-Tenant-ID header is required.", 400);
        await provisioner.EnsureGradesAttendanceLibraryEventsModuleAsync(tenant.SchemaName!, ct);
    }

    private HashSet<string> Roles()
    {
        var p = http.HttpContext?.User;
        if (p is null) return [];
        return p.FindAll("role").Concat(p.FindAll(ClaimTypes.Role)).Select(x => x.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private void Manage()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin))
            throw new ForbiddenException("Only Super Admin or School Admin can manage events.");
    }

    private Guid? TryCurrentUser()
    {
        var c = http.HttpContext?.User.FindFirst(AppConstants.Claims.UserId)
            ?? http.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        return c is not null && Guid.TryParse(c.Value, out var id) ? id : null;
    }
}
