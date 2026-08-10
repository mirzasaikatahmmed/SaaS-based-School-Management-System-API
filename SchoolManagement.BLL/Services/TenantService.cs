using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SchoolManagement.BLL.DTOs.Tenant;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Helpers;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Master;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class TenantService : ITenantService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantSchemaProvisioner _schemaProvisioner;
    private readonly IStorageService _storageService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TenantService> _logger;

    public TenantService(
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork,
        ITenantSchemaProvisioner schemaProvisioner,
        IStorageService storageService,
        IServiceScopeFactory scopeFactory,
        ILogger<TenantService> logger)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
        _schemaProvisioner = schemaProvisioner;
        _storageService = storageService;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<TenantResponseDto> CreateTenantAsync(
        CreateTenantDto request,
        CancellationToken cancellationToken = default)
    {
        var slug = request.Slug.ToLowerInvariant().Trim();

        if (await _tenantRepository.SlugExistsAsync(slug, cancellationToken))
            throw new ConflictException($"Tenant slug '{slug}' already exists.");

        var schemaName = $"{AppConstants.SchemaPrefix}{slug.Replace('-', '_')}";
        var settings = MapSettings(request.Settings, request.Name, request.MaxUsers);

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Slug = slug,
            Domain = string.IsNullOrWhiteSpace(request.Domain) ? null : request.Domain.Trim().ToLowerInvariant(),
            SchemaName = schemaName,
            IsActive = true,
            SubscriptionPlan = string.IsNullOrWhiteSpace(request.SubscriptionPlan)
                ? AppConstants.DefaultSubscriptionPlan
                : request.SubscriptionPlan,
            MaxUsers = request.MaxUsers > 0 ? request.MaxUsers : AppConstants.DefaultMaxUsers,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        tenant.SetSettings(settings);

        // 1. Create PostgreSQL schema + tables
        await _schemaProvisioner.ProvisionAsync(schemaName, cancellationToken);

        // 2. Ensure school folder in shared MinIO bucket
        await _storageService.EnsureBucketAsync(slug, cancellationToken);

        // 3. Persist tenant registry record
        await _tenantRepository.AddAsync(tenant, cancellationToken);
        await _unitOfWork.SaveMasterChangesAsync(cancellationToken);

        // 4. Seed roles + initial school admin inside tenant schema
        await SeedTenantDataAsync(schemaName, request.Admin!, cancellationToken);

        _logger.LogInformation("Tenant {Slug} provisioned with schema {Schema}", slug, schemaName);

        return MapTenant(tenant);
    }

    public async Task<TenantResponseDto> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetBySlugAsync(slug, cancellationToken)
            ?? throw new NotFoundException($"Tenant '{slug}' not found.");

        return MapTenant(tenant);
    }

    public async Task<TenantResponseDto> UpdateSettingsAsync(
        string slug,
        UpdateTenantSettingsDto request,
        CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetBySlugAsync(slug, cancellationToken)
            ?? throw new NotFoundException($"Tenant '{slug}' not found.");

        if (!string.IsNullOrWhiteSpace(request.Name))
            tenant.Name = request.Name.Trim();

        if (request.Domain is not null)
            tenant.Domain = string.IsNullOrWhiteSpace(request.Domain) ? null : request.Domain.Trim().ToLowerInvariant();

        if (!string.IsNullOrWhiteSpace(request.SubscriptionPlan))
            tenant.SubscriptionPlan = request.SubscriptionPlan;

        if (request.MaxUsers.HasValue)
            tenant.MaxUsers = request.MaxUsers.Value;

        if (request.Settings is not null)
        {
            var settings = MapSettings(request.Settings, tenant.Name, tenant.MaxUsers);
            tenant.SetSettings(settings);
        }

        tenant.UpdatedAt = DateTime.UtcNow;
        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await _unitOfWork.SaveMasterChangesAsync(cancellationToken);

        _logger.LogInformation("Tenant {Slug} settings updated", slug);
        return MapTenant(tenant);
    }

    public async Task DeactivateAsync(string slug, CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetBySlugAsync(slug, cancellationToken)
            ?? throw new NotFoundException($"Tenant '{slug}' not found.");

        tenant.IsActive = false;
        tenant.UpdatedAt = DateTime.UtcNow;
        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await _unitOfWork.SaveMasterChangesAsync(cancellationToken);

        _logger.LogInformation("Tenant {Slug} deactivated", slug);
    }

    public async Task<IReadOnlyList<TenantResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tenants = await _tenantRepository.GetAllAsync(cancellationToken);
        return tenants.Select(MapTenant).ToList();
    }

    private async Task SeedTenantDataAsync(
        string schemaName,
        CreateTenantAdminDto admin,
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<ITenantDbContextFactory>();
        await using var tenantDb = factory.Create(schemaName);

        var roles = new Dictionary<string, Role>();
        foreach (var (legacyId, name, prefix, isSystem) in AppConstants.Roles.Seed)
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
            _ = legacyId;
        }

        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Email = admin.Email.ToLowerInvariant(),
            Username = admin.Username.ToLowerInvariant(),
            Password = PasswordHelper.HashPassword(admin.Password),
            FirstName = admin.FirstName,
            LastName = admin.LastName,
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

    private static TenantSettings MapSettings(TenantSettingsDto? dto, string schoolName, int maxUsers)
    {
        dto ??= new TenantSettingsDto();
        return new TenantSettings
        {
            Features = new FeatureSettings
            {
                MaxUsers = dto.Features?.MaxUsers > 0 ? dto.Features.MaxUsers : maxUsers,
                StorageQuotaGB = dto.Features?.StorageQuotaGB > 0 ? dto.Features.StorageQuotaGB : 10,
                AllowSelfRegistration = dto.Features?.AllowSelfRegistration ?? true,
                RequireEmailVerification = dto.Features?.RequireEmailVerification ?? true
            },
            Branding = new BrandingSettings
            {
                SchoolName = string.IsNullOrWhiteSpace(dto.Branding?.SchoolName) ? schoolName : dto.Branding.SchoolName,
                LogoUrl = dto.Branding?.LogoUrl,
                PrimaryColor = string.IsNullOrWhiteSpace(dto.Branding?.PrimaryColor) ? "#1a73e8" : dto.Branding.PrimaryColor,
                Timezone = string.IsNullOrWhiteSpace(dto.Branding?.Timezone) ? "UTC" : dto.Branding.Timezone,
                Locale = string.IsNullOrWhiteSpace(dto.Branding?.Locale) ? "en-US" : dto.Branding.Locale
            },
            Security = new SecuritySettings
            {
                PasswordMinLength = dto.Security?.PasswordMinLength > 0 ? dto.Security.PasswordMinLength : 8,
                SessionTimeoutMinutes = dto.Security?.SessionTimeoutMinutes > 0 ? dto.Security.SessionTimeoutMinutes : 60,
                MaxLoginAttempts = dto.Security?.MaxLoginAttempts > 0 ? dto.Security.MaxLoginAttempts : 5,
                LockoutDurationMinutes = dto.Security?.LockoutDurationMinutes > 0 ? dto.Security.LockoutDurationMinutes : 15
            }
        };
    }

    private static TenantResponseDto MapTenant(Tenant tenant)
    {
        var settings = tenant.GetSettings();
        return new TenantResponseDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Slug = tenant.Slug,
            Domain = tenant.Domain,
            SchemaName = tenant.SchemaName,
            IsActive = tenant.IsActive,
            SubscriptionPlan = tenant.SubscriptionPlan,
            MaxUsers = tenant.MaxUsers,
            CreatedAt = tenant.CreatedAt,
            UpdatedAt = tenant.UpdatedAt,
            Settings = new TenantSettingsDto
            {
                Features = new FeatureSettingsDto
                {
                    MaxUsers = settings.Features.MaxUsers,
                    StorageQuotaGB = settings.Features.StorageQuotaGB,
                    AllowSelfRegistration = settings.Features.AllowSelfRegistration,
                    RequireEmailVerification = settings.Features.RequireEmailVerification
                },
                Branding = new BrandingSettingsDto
                {
                    SchoolName = settings.Branding.SchoolName,
                    LogoUrl = settings.Branding.LogoUrl,
                    PrimaryColor = settings.Branding.PrimaryColor,
                    Timezone = settings.Branding.Timezone,
                    Locale = settings.Branding.Locale
                },
                Security = new SecuritySettingsDto
                {
                    PasswordMinLength = settings.Security.PasswordMinLength,
                    SessionTimeoutMinutes = settings.Security.SessionTimeoutMinutes,
                    MaxLoginAttempts = settings.Security.MaxLoginAttempts,
                    LockoutDurationMinutes = settings.Security.LockoutDurationMinutes
                }
            }
        };
    }
}
