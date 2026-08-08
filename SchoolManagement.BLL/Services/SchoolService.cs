using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolManagement.BLL.DTOs.School;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Helpers;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Master;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.BLL.Services;

public class SchoolService : ISchoolService
{
    private readonly MasterDbContext _masterDb;
    private readonly ISchoolRepository _schoolRepository;
    private readonly ITenantSchemaProvisioner _schemaProvisioner;
    private readonly IStorageService _storageService;
    private readonly ITenantDbContextFactory _tenantDbFactory;
    private readonly ILogger<SchoolService> _logger;

    public SchoolService(
        MasterDbContext masterDb,
        ISchoolRepository schoolRepository,
        ITenantSchemaProvisioner schemaProvisioner,
        IStorageService storageService,
        ITenantDbContextFactory tenantDbFactory,
        ILogger<SchoolService> logger)
    {
        _masterDb = masterDb;
        _schoolRepository = schoolRepository;
        _schemaProvisioner = schemaProvisioner;
        _storageService = storageService;
        _tenantDbFactory = tenantDbFactory;
        _logger = logger;
    }

    public async Task<SchoolListResponseDto> GetSchoolsAsync(
        SchoolSearchFilter filter,
        CancellationToken cancellationToken = default)
    {
        var (items, total) = await _schoolRepository.SearchAsync(filter, cancellationToken);
        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize is < 1 or > 200 ? 20 : filter.PageSize;
        var offset = (page - 1) * pageSize;

        var dtos = new List<SchoolResponseDto>();
        for (var i = 0; i < items.Count; i++)
        {
            dtos.Add(await MapToResponseAsync(items[i], offset + i + 1, cancellationToken));
        }

        return new SchoolListResponseDto
        {
            Items = dtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            TotalPages = pageSize <= 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize)
        };
    }

    public async Task<SchoolResponseDto> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var school = await GetSchoolOrThrowAsync(slug, cancellationToken);
        return await MapToResponseAsync(school, 1, cancellationToken);
    }

    public async Task<SchoolResponseDto> CreateSchoolAsync(
        CreateSchoolDto dto,
        CancellationToken cancellationToken = default)
    {
        var slug = dto.Slug.Trim().ToLowerInvariant();
        if (await _schoolRepository.SlugExistsAsync(slug, cancellationToken))
            throw new ConflictException($"School slug '{slug}' already exists.");

        var schemaName = $"{AppConstants.SchemaPrefix}{slug.Replace('-', '_')}";
        var schemaCreated = false;
        var bucketCreated = false;
        Tenant? tenant = null;

        try
        {
            // Schema + bucket first (separate connections need committed DDL)
            await _schemaProvisioner.ProvisionAsync(schemaName, cancellationToken);
            schemaCreated = true;

            await _storageService.EnsureBucketAsync(slug, cancellationToken);
            bucketCreated = true;

            await using var transaction = await _masterDb.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                tenant = new Tenant
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name.Trim(),
                    Slug = slug,
                    Domain = string.IsNullOrWhiteSpace(dto.Domain) ? null : dto.Domain.Trim().ToLowerInvariant(),
                    SchemaName = schemaName,
                    IsActive = true,
                    SubscriptionPlan = string.IsNullOrWhiteSpace(dto.SubscriptionPlan) ? "basic" : dto.SubscriptionPlan.Trim(),
                    MaxUsers = dto.MaxUsers > 0 ? dto.MaxUsers : 100,
                    Phone = dto.Phone,
                    Email = dto.Email?.Trim().ToLowerInvariant(),
                    Website = dto.Website,
                    Address = dto.Address,
                    City = dto.City,
                    State = dto.State,
                    Country = string.IsNullOrWhiteSpace(dto.Country) ? "Bangladesh" : dto.Country,
                    Currency = string.IsNullOrWhiteSpace(dto.Currency) ? "BDT" : dto.Currency,
                    CurrencySymbol = string.IsNullOrWhiteSpace(dto.CurrencySymbol) ? "৳" : dto.CurrencySymbol,
                    Timezone = string.IsNullOrWhiteSpace(dto.Timezone) ? "Asia/Dhaka" : dto.Timezone,
                    Locale = string.IsNullOrWhiteSpace(dto.Locale) ? "en-US" : dto.Locale!,
                    EstablishedYear = dto.EstablishedYear,
                    SchoolType = dto.SchoolType,
                    SubscriptionExpiresAt = dto.SubscriptionExpiresAt,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var settings = new TenantSettings
                {
                    Features = new FeatureSettings
                    {
                        MaxUsers = tenant.MaxUsers,
                        StorageQuotaGB = 10,
                        AllowSelfRegistration = false,
                        RequireEmailVerification = true
                    },
                    Branding = new BrandingSettings
                    {
                        SchoolName = tenant.Name,
                        PrimaryColor = "#1a73e8",
                        Timezone = tenant.Timezone,
                        Locale = tenant.Locale
                    },
                    Security = new SecuritySettings()
                };
                tenant.SetSettings(settings);

                await _schoolRepository.AddAsync(tenant, cancellationToken);
                await _masterDb.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                try { await _masterDb.Database.RollbackTransactionAsync(cancellationToken); } catch { /* ignore */ }
                throw;
            }

            await SeedSchoolAdminAsync(schemaName, dto, cancellationToken);

            _logger.LogInformation("School {Slug} provisioned (schema {Schema})", slug, schemaName);
            return await MapToResponseAsync(tenant, 1, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "School provisioning failed for slug {Slug} — rolling back", slug);

            if (schemaCreated)
            {
                try { await _schemaProvisioner.DropSchemaAsync(schemaName, cancellationToken); }
                catch (Exception dropEx) { _logger.LogWarning(dropEx, "Failed to drop schema {Schema}", schemaName); }
            }

            if (bucketCreated)
            {
                try { await _storageService.DeleteBucketAsync(slug, cancellationToken); }
                catch (Exception dropEx) { _logger.LogWarning(dropEx, "Failed to delete bucket for {Slug}", slug); }
            }

            if (tenant is not null)
            {
                var existing = await _masterDb.Tenants.FirstOrDefaultAsync(t => t.Id == tenant.Id, cancellationToken);
                if (existing is not null)
                {
                    _masterDb.Tenants.Remove(existing);
                    await _masterDb.SaveChangesAsync(cancellationToken);
                }
            }

            throw new AppException($"School provisioning failed: {ex.Message}", 500);
        }
    }

    public async Task<SchoolResponseDto> UpdateSchoolAsync(
        string slug,
        UpdateSchoolDto dto,
        CancellationToken cancellationToken = default)
    {
        var school = await GetSchoolOrThrowAsync(slug, cancellationToken);

        if (!string.IsNullOrWhiteSpace(dto.Name)) school.Name = dto.Name.Trim();
        if (dto.Domain is not null)
            school.Domain = string.IsNullOrWhiteSpace(dto.Domain) ? null : dto.Domain.Trim().ToLowerInvariant();
        if (dto.Phone is not null) school.Phone = dto.Phone;
        if (dto.Email is not null) school.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim().ToLowerInvariant();
        if (dto.Website is not null) school.Website = dto.Website;
        if (dto.Address is not null) school.Address = dto.Address;
        if (dto.City is not null) school.City = dto.City;
        if (dto.State is not null) school.State = dto.State;
        if (!string.IsNullOrWhiteSpace(dto.Country)) school.Country = dto.Country;
        if (!string.IsNullOrWhiteSpace(dto.Currency)) school.Currency = dto.Currency;
        if (!string.IsNullOrWhiteSpace(dto.CurrencySymbol)) school.CurrencySymbol = dto.CurrencySymbol;
        if (!string.IsNullOrWhiteSpace(dto.Timezone)) school.Timezone = dto.Timezone;
        if (!string.IsNullOrWhiteSpace(dto.Locale)) school.Locale = dto.Locale;
        if (dto.EstablishedYear.HasValue) school.EstablishedYear = dto.EstablishedYear;
        if (dto.SchoolType is not null) school.SchoolType = dto.SchoolType;
        if (!string.IsNullOrWhiteSpace(dto.SubscriptionPlan)) school.SubscriptionPlan = dto.SubscriptionPlan;
        if (dto.SubscriptionExpiresAt.HasValue) school.SubscriptionExpiresAt = dto.SubscriptionExpiresAt;
        if (dto.MaxUsers.HasValue)
        {
            school.MaxUsers = dto.MaxUsers.Value;
            var settings = school.GetSettings();
            settings.Features.MaxUsers = dto.MaxUsers.Value;
            school.SetSettings(settings);
        }

        school.UpdatedAt = DateTime.UtcNow;
        await _schoolRepository.UpdateAsync(school, cancellationToken);
        await _masterDb.SaveChangesAsync(cancellationToken);

        return await MapToResponseAsync(school, 1, cancellationToken);
    }

    public async Task DeactivateAsync(string slug, CancellationToken cancellationToken = default)
    {
        var school = await GetSchoolOrThrowAsync(slug, cancellationToken);
        school.IsActive = false;
        school.UpdatedAt = DateTime.UtcNow;
        await _schoolRepository.UpdateAsync(school, cancellationToken);
        await _masterDb.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("School {Slug} deactivated", slug);
    }

    public async Task ActivateAsync(string slug, CancellationToken cancellationToken = default)
    {
        var school = await GetSchoolOrThrowAsync(slug, cancellationToken);
        school.IsActive = true;
        school.UpdatedAt = DateTime.UtcNow;
        await _schoolRepository.UpdateAsync(school, cancellationToken);
        await _masterDb.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("School {Slug} activated", slug);
    }

    public async Task<SchoolResponseDto> UploadLogoAsync(
        string slug,
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var school = await GetSchoolOrThrowAsync(slug, cancellationToken);
        var objectKey = await _storageService.UploadFileAsync(
            slug, AppConstants.StorageFolders.Logo, fileStream, fileName, contentType, cancellationToken);

        school.LogoUrl = objectKey;
        var settings = school.GetSettings();
        settings.Branding.LogoUrl = objectKey;
        school.SetSettings(settings);
        school.UpdatedAt = DateTime.UtcNow;

        await _schoolRepository.UpdateAsync(school, cancellationToken);
        await _masterDb.SaveChangesAsync(cancellationToken);

        return await MapToResponseAsync(school, 1, cancellationToken);
    }

    public async Task<SchoolSettingsDto> GetSettingsAsync(string slug, CancellationToken cancellationToken = default)
    {
        var school = await GetSchoolOrThrowAsync(slug, cancellationToken);
        return MapSettingsDto(school);
    }

    public async Task<SchoolSettingsDto> UpdateSettingsAsync(
        string slug,
        SchoolSettingsDto dto,
        CancellationToken cancellationToken = default)
    {
        var school = await GetSchoolOrThrowAsync(slug, cancellationToken);
        var settings = new TenantSettings
        {
            Features = new FeatureSettings
            {
                MaxUsers = dto.Features.MaxUsers,
                StorageQuotaGB = dto.Features.StorageQuotaGB,
                AllowSelfRegistration = dto.Features.AllowSelfRegistration,
                RequireEmailVerification = dto.Features.RequireEmailVerification
            },
            Branding = new BrandingSettings
            {
                SchoolName = dto.Branding.SchoolName ?? school.Name,
                LogoUrl = dto.Branding.LogoUrl ?? school.LogoUrl,
                PrimaryColor = dto.Branding.PrimaryColor,
                Timezone = dto.Branding.Timezone,
                Locale = dto.Branding.Locale
            },
            Security = new SecuritySettings
            {
                PasswordMinLength = dto.Security.PasswordMinLength,
                SessionTimeoutMinutes = dto.Security.SessionTimeoutMinutes,
                MaxLoginAttempts = dto.Security.MaxLoginAttempts,
                LockoutDurationMinutes = dto.Security.LockoutDurationMinutes
            }
        };

        school.MaxUsers = dto.Features.MaxUsers;
        school.Timezone = dto.Branding.Timezone;
        school.Locale = dto.Branding.Locale;
        if (!string.IsNullOrWhiteSpace(dto.Branding.SchoolName))
            school.Name = dto.Branding.SchoolName!;
        school.SetSettings(settings);

        await _schoolRepository.UpdateAsync(school, cancellationToken);
        await _masterDb.SaveChangesAsync(cancellationToken);
        return MapSettingsDto(school);
    }

    public async Task<SchoolStatsDto> GetStatsAsync(string slug, CancellationToken cancellationToken = default)
    {
        var school = await GetSchoolOrThrowAsync(slug, cancellationToken);
        var settings = school.GetSettings();

        var totalUsers = 0;
        var activeUsers = 0;
        try
        {
            await using var tenantDb = _tenantDbFactory.Create(school.SchemaName);
            totalUsers = await tenantDb.Users.CountAsync(cancellationToken);
            activeUsers = await tenantDb.Users.CountAsync(u => u.Active, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not query users for school {Slug}", slug);
        }

        var storageBytes = await _storageService.GetBucketSizeBytesAsync(slug, cancellationToken);
        var status = ResolveSubscriptionStatus(school.SubscriptionExpiresAt);

        return new SchoolStatsDto
        {
            Slug = school.Slug,
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            StorageUsedBytes = storageBytes,
            StorageUsedMB = Math.Round(storageBytes / (1024.0 * 1024.0), 2),
            StorageQuotaGB = settings.Features.StorageQuotaGB,
            SubscriptionPlan = school.SubscriptionPlan,
            SubscriptionExpiresAt = school.SubscriptionExpiresAt,
            SubscriptionStatus = status
        };
    }

    public async Task<(byte[] Content, string ContentType, string FileName)> ExportAsync(
        SchoolSearchFilter filter,
        string format,
        CancellationToken cancellationToken = default)
    {
        filter.Page = 1;
        filter.PageSize = 10_000;
        var (items, _) = await _schoolRepository.SearchAsync(filter, cancellationToken);

        var fmt = format.Trim().ToLowerInvariant();
        return fmt switch
        {
            "csv" or "excel" => BuildCsvExport(items, fmt == "excel"),
            "pdf" => BuildPdfExport(items),
            _ => throw new AppException("Unsupported export format. Use csv, excel, or pdf.", 400)
        };
    }

    private async Task SeedSchoolAdminAsync(string schemaName, CreateSchoolDto dto, CancellationToken cancellationToken)
    {
        await using var tenantDb = _tenantDbFactory.Create(schemaName);

        var roles = new Dictionary<string, Role>();
        foreach (var (_, name, prefix, isSystem) in AppConstants.Roles.Seed)
        {
            if (prefix == AppConstants.Roles.SuperAdmin)
                continue;

            var role = new Role
            {
                Id = Guid.NewGuid(),
                Name = name,
                Prefix = prefix,
                IsSystem = isSystem,
                Description = $"{name} role",
                CreatedAt = DateTime.UtcNow
            };
            await tenantDb.Roles.AddAsync(role, cancellationToken);
            roles[prefix] = role;
        }

        var username = dto.AdminEmail.Split('@')[0].ToLowerInvariant();
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Email = dto.AdminEmail.ToLowerInvariant(),
            Username = username,
            Password = PasswordHelper.HashPassword(dto.AdminPassword),
            FirstName = dto.AdminFirstName,
            LastName = dto.AdminLastName,
            Active = true,
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await tenantDb.Users.AddAsync(adminUser, cancellationToken);
        await tenantDb.UserRoles.AddAsync(new UserRole
        {
            UserId = adminUser.Id,
            RoleId = roles[AppConstants.Roles.Admin].Id
        }, cancellationToken);

        await tenantDb.SaveChangesAsync(cancellationToken);
    }

    private async Task<Tenant> GetSchoolOrThrowAsync(string slug, CancellationToken cancellationToken)
    {
        return await _schoolRepository.GetBySlugAsync(slug, cancellationToken)
            ?? throw new NotFoundException($"School '{slug}' not found.");
    }

    private async Task<SchoolResponseDto> MapToResponseAsync(Tenant school, int sl, CancellationToken cancellationToken)
    {
        string? logoPresigned = null;
        if (!string.IsNullOrWhiteSpace(school.LogoUrl))
        {
            try
            {
                logoPresigned = await _storageService.GetPresignedUrlAsync(school.Slug, school.LogoUrl, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not generate logo URL for {Slug}", school.Slug);
                logoPresigned = school.LogoUrl;
            }
        }

        return new SchoolResponseDto
        {
            Id = school.Id,
            Sl = sl,
            Name = school.Name,
            Slug = school.Slug,
            SchemaName = school.SchemaName,
            Domain = school.Domain,
            Phone = school.Phone,
            Email = school.Email,
            Website = school.Website,
            Address = school.Address,
            City = school.City,
            State = school.State,
            Country = school.Country,
            Currency = school.Currency,
            CurrencySymbol = school.CurrencySymbol,
            Timezone = school.Timezone,
            Locale = school.Locale,
            LogoUrl = logoPresigned,
            EstablishedYear = school.EstablishedYear,
            SchoolType = school.SchoolType,
            SubscriptionPlan = school.SubscriptionPlan,
            SubscriptionExpiresAt = school.SubscriptionExpiresAt,
            IsActive = school.IsActive,
            MaxUsers = school.MaxUsers,
            CreatedAt = school.CreatedAt
        };
    }

    private static SchoolSettingsDto MapSettingsDto(Tenant school)
    {
        var s = school.GetSettings();
        return new SchoolSettingsDto
        {
            Features = new SchoolSettingsDto.FeatureSettings
            {
                MaxUsers = s.Features.MaxUsers,
                StorageQuotaGB = s.Features.StorageQuotaGB,
                AllowSelfRegistration = s.Features.AllowSelfRegistration,
                RequireEmailVerification = s.Features.RequireEmailVerification
            },
            Branding = new SchoolSettingsDto.BrandingSettings
            {
                SchoolName = s.Branding.SchoolName,
                LogoUrl = s.Branding.LogoUrl ?? school.LogoUrl,
                PrimaryColor = s.Branding.PrimaryColor,
                Timezone = s.Branding.Timezone,
                Locale = s.Branding.Locale
            },
            Security = new SchoolSettingsDto.SecuritySettings
            {
                PasswordMinLength = s.Security.PasswordMinLength,
                SessionTimeoutMinutes = s.Security.SessionTimeoutMinutes,
                MaxLoginAttempts = s.Security.MaxLoginAttempts,
                LockoutDurationMinutes = s.Security.LockoutDurationMinutes
            }
        };
    }

    private static string ResolveSubscriptionStatus(DateTime? expiresAt)
    {
        if (!expiresAt.HasValue) return "active";
        var days = (expiresAt.Value.Date - DateTime.UtcNow.Date).TotalDays;
        if (days < 0) return "expired";
        if (days <= 30) return "expiring_soon";
        return "active";
    }

    private static (byte[] Content, string ContentType, string FileName) BuildCsvExport(
        IReadOnlyList<Tenant> items, bool asExcel)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Sl,Name,Slug,City,State,Country,SchoolType,SubscriptionPlan,IsActive,CreatedAt");
        for (var i = 0; i < items.Count; i++)
        {
            var t = items[i];
            sb.AppendLine(string.Join(',',
                i + 1,
                Csv(t.Name),
                Csv(t.Slug),
                Csv(t.City),
                Csv(t.State),
                Csv(t.Country),
                Csv(t.SchoolType),
                Csv(t.SubscriptionPlan),
                t.IsActive ? "1" : "0",
                t.CreatedAt.ToString("u", CultureInfo.InvariantCulture)));
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        if (asExcel)
            return (bytes, "application/vnd.ms-excel", $"schools-{DateTime.UtcNow:yyyyMMdd}.xls");

        return (bytes, "text/csv", $"schools-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    private static string Csv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        var escaped = value.Replace("\"", "\"\"");
        return $"\"{escaped}\"";
    }

    private static (byte[] Content, string ContentType, string FileName) BuildPdfExport(IReadOnlyList<Tenant> items)
    {
        // Minimal valid PDF listing schools (no external package)
        var lines = new List<string> { "School Management — Schools Export", "" };
        for (var i = 0; i < items.Count; i++)
        {
            var t = items[i];
            lines.Add($"{i + 1}. {t.Name} ({t.Slug}) — {(t.IsActive ? "Active" : "Inactive")}");
        }

        var content = string.Join('\n', lines);
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream, Encoding.ASCII, leaveOpen: true);
        // Very small PDF using Helvetica text via content stream
        var escaped = content
            .Replace("\\", "\\\\")
            .Replace("(", "\\(")
            .Replace(")", "\\)");
        var textOps = new StringBuilder();
        textOps.Append("BT /F1 10 Tf 50 750 Td 14 TL ");
        foreach (var line in lines)
        {
            var safe = line.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
            textOps.Append($"({safe}) Tj T* ");
        }
        textOps.Append("ET");
        var streamContent = textOps.ToString();
        _ = escaped;

        var objects = new List<string>
        {
            "1 0 obj<< /Type /Catalog /Pages 2 0 R >>endobj\n",
            "2 0 obj<< /Type /Pages /Kids [3 0 R] /Count 1 >>endobj\n",
            "3 0 obj<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources<< /Font<< /F1 5 0 R >> >> >>endobj\n",
            $"4 0 obj<< /Length {streamContent.Length} >>stream\n{streamContent}\nendstream\nendobj\n",
            "5 0 obj<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>endobj\n"
        };

        var pdf = new StringBuilder();
        pdf.Append("%PDF-1.4\n");
        var offsets = new List<long> { 0 };
        foreach (var obj in objects)
        {
            offsets.Add(Encoding.ASCII.GetByteCount(pdf.ToString()));
            pdf.Append(obj);
        }

        var xrefPos = Encoding.ASCII.GetByteCount(pdf.ToString());
        pdf.Append($"xref\n0 {objects.Count + 1}\n");
        pdf.Append("0000000000 65535 f \n");
        for (var i = 1; i < offsets.Count; i++)
            pdf.Append($"{offsets[i]:D10} 00000 n \n");
        pdf.Append($"trailer<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xrefPos}\n%%EOF");

        var bytes = Encoding.ASCII.GetBytes(pdf.ToString());
        return (bytes, "application/pdf", $"schools-{DateTime.UtcNow:yyyyMMdd}.pdf");
    }
}
