using SchoolManagement.BLL.DTOs.Settings;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.DAL.Entities.Master;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

/// <summary>
/// Platform-wide settings — MasterDbContext only. No tenant schema is required
/// for GET/PATCH, since these are SuperAdmin-scoped global values.
/// </summary>
public class GlobalSettingsService(IUnitOfWork uow) : IGlobalSettingsService
{
    public async Task<GlobalSettingsResponseDto> GetAsync(CancellationToken ct = default)
        => Map(await EnsureExistsAsync(ct));

    public async Task<GlobalSettingsResponseDto> UpdateGeneralAsync(UpdateGlobalGeneralDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.SiteName))
            throw new AppException("Site name is required.", 400);

        var settings = await EnsureExistsAsync(ct);
        settings.SiteName = dto.SiteName.Trim();
        settings.SiteTitle = dto.SiteTitle?.Trim();
        settings.AdminEmail = dto.AdminEmail?.Trim().ToLowerInvariant();
        settings.SupportPhone = dto.SupportPhone?.Trim();
        settings.DefaultTimezone = string.IsNullOrWhiteSpace(dto.DefaultTimezone) ? settings.DefaultTimezone : dto.DefaultTimezone.Trim();
        settings.DefaultCurrency = string.IsNullOrWhiteSpace(dto.DefaultCurrency) ? settings.DefaultCurrency : dto.DefaultCurrency.Trim();
        settings.DefaultCurrencySymbol = string.IsNullOrWhiteSpace(dto.DefaultCurrencySymbol) ? settings.DefaultCurrencySymbol : dto.DefaultCurrencySymbol.Trim();
        settings.DefaultLocale = string.IsNullOrWhiteSpace(dto.DefaultLocale) ? settings.DefaultLocale : dto.DefaultLocale.Trim();
        settings.DefaultDateFormat = string.IsNullOrWhiteSpace(dto.DefaultDateFormat) ? settings.DefaultDateFormat : dto.DefaultDateFormat.Trim();
        settings.MaintenanceMode = dto.MaintenanceMode;
        settings.MaintenanceMessage = dto.MaintenanceMessage?.Trim();
        settings.UpdatedAt = DateTime.UtcNow;

        await uow.GlobalSettings.UpdateAsync(settings, ct);
        await uow.SaveMasterChangesAsync(ct);
        return Map(settings);
    }

    public async Task<GlobalSettingsResponseDto> UpdateUploadFileAsync(UpdateGlobalUploadFileDto dto, CancellationToken ct = default)
    {
        if (dto.MaxUploadSizeMb <= 0)
            throw new AppException("Max upload size must be greater than zero.", 400);
        if (string.IsNullOrWhiteSpace(dto.AllowedFileTypes))
            throw new AppException("Allowed file types are required.", 400);

        var settings = await EnsureExistsAsync(ct);
        settings.MaxUploadSizeMb = dto.MaxUploadSizeMb;
        settings.AllowedFileTypes = string.Join(",", dto.AllowedFileTypes
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => x.TrimStart('.').ToLowerInvariant()));
        settings.UpdatedAt = DateTime.UtcNow;

        await uow.GlobalSettings.UpdateAsync(settings, ct);
        await uow.SaveMasterChangesAsync(ct);
        return Map(settings);
    }

    private async Task<GlobalSettings> EnsureExistsAsync(CancellationToken ct)
    {
        var existing = await uow.GlobalSettings.GetAsync(ct);
        if (existing is not null)
            return existing;

        var created = new GlobalSettings
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await uow.GlobalSettings.AddAsync(created, ct);
        await uow.SaveMasterChangesAsync(ct);
        return created;
    }

    private static GlobalSettingsResponseDto Map(GlobalSettings s) => new()
    {
        Id = s.Id,
        SiteName = s.SiteName,
        SiteTitle = s.SiteTitle,
        SiteLogoUrl = s.SiteLogoUrl,
        SiteFaviconUrl = s.SiteFaviconUrl,
        AdminEmail = s.AdminEmail,
        SupportPhone = s.SupportPhone,
        DefaultTimezone = s.DefaultTimezone,
        DefaultCurrency = s.DefaultCurrency,
        DefaultCurrencySymbol = s.DefaultCurrencySymbol,
        DefaultLocale = s.DefaultLocale,
        DefaultDateFormat = s.DefaultDateFormat,
        MaintenanceMode = s.MaintenanceMode,
        MaintenanceMessage = s.MaintenanceMessage,
        MaxUploadSizeMb = s.MaxUploadSizeMb,
        AllowedFileTypes = s.AllowedFileTypes,
        UpdatedAt = s.UpdatedAt
    };
}
