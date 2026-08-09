using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.Settings;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Master;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Implementations;
using SchoolManagement.DAL.Repositories.Interfaces;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

/// <summary>
/// School (tenant) settings live in the tenant schema's school_settings table.
/// SuperAdmin can manage any school by slug; School Admin can only manage their own tenant.
/// </summary>
public class SchoolSettingsService(
    IUnitOfWork uow,
    ITenantContext tenantContext,
    ITenantDbContextFactory tenantDbFactory,
    ITenantSchemaProvisioner provisioner,
    IStorageService storage,
    IHttpContextAccessor http) : ISchoolSettingsService
{
    public async Task<SchoolListResponseDto> GetSchoolListAsync(SchoolSearchFilter filter, CancellationToken ct = default)
    {
        RequireSuperAdmin();
        var (items, total) = await uow.Schools.SearchAsync(filter, ct);
        var page = filter.Page < 1 ? 1 : filter.Page;
        var size = filter.PageSize is < 1 or > 200 ? 20 : filter.PageSize;
        return new SchoolListResponseDto
        {
            Data = items.Select((t, i) => new SchoolListItemDto
            {
                Id = t.Id,
                Sl = (page - 1) * size + i + 1,
                Name = t.Name,
                Slug = t.Slug,
                SchemaName = t.SchemaName,
                IsActive = t.IsActive,
                City = t.City,
                State = t.State,
                SubscriptionPlan = t.SubscriptionPlan,
                CreatedAt = t.CreatedAt
            }).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = size,
            TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)size)
        };
    }

    public async Task<SchoolSettingsResponseDto> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var tenant = await ResolveTenantAsync(slug, ct);
        await using var db = tenantDbFactory.Create(tenant.SchemaName);
        await provisioner.EnsureMessageAndSettingsModuleAsync(tenant.SchemaName, ct);
        var repo = new SchoolSettingsRepository(db);
        var settings = await repo.GetOrCreateAsync(ct);
        return Map(settings, tenant);
    }

    public async Task<SchoolSettingsResponseDto> UpdateGeneralAsync(string slug, UpdateSchoolGeneralDto dto, CancellationToken ct = default)
    {
        var tenant = await ResolveTenantAsync(slug, ct);
        await using var db = tenantDbFactory.Create(tenant.SchemaName);
        await provisioner.EnsureMessageAndSettingsModuleAsync(tenant.SchemaName, ct);
        var repo = new SchoolSettingsRepository(db);
        var settings = await repo.GetOrCreateAsync(ct);

        settings.SchoolName = dto.SchoolName?.Trim();
        settings.SchoolCode = dto.SchoolCode?.Trim();
        settings.Address = dto.Address?.Trim();
        settings.Phone = dto.Phone?.Trim();
        settings.Email = dto.Email?.Trim().ToLowerInvariant();
        settings.Website = dto.Website?.Trim();
        settings.Timezone = string.IsNullOrWhiteSpace(dto.Timezone) ? settings.Timezone : dto.Timezone.Trim();
        settings.Currency = string.IsNullOrWhiteSpace(dto.Currency) ? settings.Currency : dto.Currency.Trim();
        settings.CurrencySymbol = string.IsNullOrWhiteSpace(dto.CurrencySymbol) ? settings.CurrencySymbol : dto.CurrencySymbol.Trim();
        settings.DateFormat = string.IsNullOrWhiteSpace(dto.DateFormat) ? settings.DateFormat : dto.DateFormat.Trim();
        settings.Language = string.IsNullOrWhiteSpace(dto.Language) ? settings.Language : dto.Language.Trim();
        settings.UpdatedAt = DateTime.UtcNow;

        await repo.UpdateAsync(settings, ct);
        await db.SaveChangesAsync(ct);
        return Map(settings, tenant);
    }

    public async Task<SchoolSettingsResponseDto> UpdateStudentPanelAsync(string slug, UpdateStudentPanelDto dto, CancellationToken ct = default)
    {
        var tenant = await ResolveTenantAsync(slug, ct);
        await using var db = tenantDbFactory.Create(tenant.SchemaName);
        await provisioner.EnsureMessageAndSettingsModuleAsync(tenant.SchemaName, ct);
        var repo = new SchoolSettingsRepository(db);
        var settings = await repo.GetOrCreateAsync(ct);

        settings.AllowStudentLogin = dto.AllowStudentLogin;
        settings.AllowGuardianLogin = dto.AllowGuardianLogin;
        settings.ShowFeesInStudentPanel = dto.ShowFeesInStudentPanel;
        settings.ShowAttendanceInStudentPanel = dto.ShowAttendanceInStudentPanel;
        settings.ShowResultInStudentPanel = dto.ShowResultInStudentPanel;
        settings.StudentPanelNoticeMessage = dto.StudentPanelNoticeMessage?.Trim();
        settings.UpdatedAt = DateTime.UtcNow;

        await repo.UpdateAsync(settings, ct);
        await db.SaveChangesAsync(ct);
        return Map(settings, tenant);
    }

    public async Task<SchoolSettingsResponseDto> UpdatePaymentAsync(string slug, PaymentSettingsDto dto, CancellationToken ct = default)
    {
        var tenant = await ResolveTenantAsync(slug, ct);
        await using var db = tenantDbFactory.Create(tenant.SchemaName);
        await provisioner.EnsureMessageAndSettingsModuleAsync(tenant.SchemaName, ct);
        var repo = new SchoolSettingsRepository(db);
        var settings = await repo.GetOrCreateAsync(ct);

        var gateways = dto.Gateways ?? [];
        settings.PaymentGateways = JsonSerializer.Serialize(gateways);
        settings.ActiveGateways = JsonSerializer.Serialize(gateways.Where(g => g.Enabled).Select(g => g.Key).ToList());
        settings.UpdatedAt = DateTime.UtcNow;

        await repo.UpdateAsync(settings, ct);
        await db.SaveChangesAsync(ct);
        return Map(settings, tenant);
    }

    public async Task<SchoolSettingsResponseDto> UploadLogoAsync(
        string slug, string type, Stream stream, string fileName, string contentType, CancellationToken ct = default)
    {
        if (!LogoTypes.IsValid(type))
            throw new AppException($"Invalid logo type '{type}'. Must be one of: {string.Join(", ", LogoTypes.All)}.", 400);

        var tenant = await ResolveTenantAsync(slug, ct);
        await using var db = tenantDbFactory.Create(tenant.SchemaName);
        await provisioner.EnsureMessageAndSettingsModuleAsync(tenant.SchemaName, ct);
        var repo = new SchoolSettingsRepository(db);
        var settings = await repo.GetOrCreateAsync(ct);

        var folder = $"{AppConstants.StorageFolders.SettingsLogos}/{type.Trim().ToLowerInvariant()}";
        var objectKey = await storage.UploadFileAsync(tenant.Slug, folder, stream, fileName, contentType, ct);

        switch (type.Trim().ToLowerInvariant())
        {
            case LogoTypes.System: settings.SystemLogoUrl = objectKey; break;
            case LogoTypes.Text: settings.TextLogoUrl = objectKey; break;
            case LogoTypes.Printing: settings.PrintingLogoUrl = objectKey; break;
            case LogoTypes.ReportCard: settings.ReportCardLogoUrl = objectKey; break;
        }
        settings.UpdatedAt = DateTime.UtcNow;

        await repo.UpdateAsync(settings, ct);
        await db.SaveChangesAsync(ct);
        return Map(settings, tenant);
    }

    private async Task<Tenant> ResolveTenantAsync(string slug, CancellationToken ct)
    {
        var tenant = await uow.Tenants.GetBySlugAsync(slug.Trim().ToLowerInvariant(), ct)
            ?? throw new NotFoundException($"School '{slug}' not found.");
        EnsureCanAccessSchool(tenant.Slug);
        return tenant;
    }

    private void EnsureCanAccessSchool(string slug)
    {
        if (Roles().Contains(AppConstants.Roles.SuperAdmin))
            return;

        if (Roles().Contains(AppConstants.Roles.Admin))
        {
            if (string.IsNullOrEmpty(tenantContext.TenantSlug))
                throw new ForbiddenException("X-Tenant-ID header is required for school admin access.");
            if (!string.Equals(tenantContext.TenantSlug, slug, StringComparison.OrdinalIgnoreCase))
                throw new ForbiddenException("You can only access your own school's settings.");
            return;
        }

        throw new ForbiddenException("You do not have permission to access school settings.");
    }

    private void RequireSuperAdmin()
    {
        if (!Roles().Contains(AppConstants.Roles.SuperAdmin))
            throw new ForbiddenException("Only Super Admin can list schools.");
    }

    private HashSet<string> Roles()
    {
        var p = http.HttpContext?.User;
        if (p is null) return [];
        return p.FindAll("role").Concat(p.FindAll(ClaimTypes.Role)).Select(x => x.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static SchoolSettingsResponseDto Map(SchoolSettings s, Tenant tenant) => new()
    {
        Id = s.Id,
        Branch = tenant.Name,
        SchoolName = s.SchoolName ?? tenant.Name,
        SchoolCode = s.SchoolCode,
        Address = s.Address ?? tenant.Address,
        Phone = s.Phone ?? tenant.Phone,
        Email = s.Email ?? tenant.Email,
        Website = s.Website ?? tenant.Website,
        Timezone = s.Timezone,
        Currency = s.Currency,
        CurrencySymbol = s.CurrencySymbol,
        DateFormat = s.DateFormat,
        Language = s.Language,
        AllowStudentLogin = s.AllowStudentLogin,
        AllowGuardianLogin = s.AllowGuardianLogin,
        ShowFeesInStudentPanel = s.ShowFeesInStudentPanel,
        ShowAttendanceInStudentPanel = s.ShowAttendanceInStudentPanel,
        ShowResultInStudentPanel = s.ShowResultInStudentPanel,
        StudentPanelNoticeMessage = s.StudentPanelNoticeMessage,
        SystemLogoUrl = s.SystemLogoUrl,
        TextLogoUrl = s.TextLogoUrl,
        PrintingLogoUrl = s.PrintingLogoUrl,
        ReportCardLogoUrl = s.ReportCardLogoUrl,
        PaymentGateways = DeserializeGateways(s.PaymentGateways),
        ActiveGateways = DeserializeStringList(s.ActiveGateways),
        UpdatedAt = s.UpdatedAt
    };

    private static List<PaymentGatewayConfigDto> DeserializeGateways(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<PaymentGatewayConfigDto>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static List<string> DeserializeStringList(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }
}
