using Microsoft.Extensions.Logging;
using SchoolManagement.BLL.DTOs.Auth;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Helpers;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantRepository _tenantRepository;
    private readonly ITenantContext _tenantContext;
    private readonly JwtHelper _jwtHelper;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUnitOfWork unitOfWork,
        ITenantRepository tenantRepository,
        ITenantContext tenantContext,
        JwtHelper jwtHelper,
        ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantRepository = tenantRepository;
        _tenantContext = tenantContext;
        _jwtHelper = jwtHelper;
        _logger = logger;
    }

    public async Task<LoginResponseDto> LoginAsync(
        LoginRequestDto request,
        string? ipAddress,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        var superAdmin = await _tenantRepository.GetSuperAdminByEmailAsync(request.Email, cancellationToken);
        if (superAdmin is not null)
        {
            if (!PasswordHelper.VerifyPassword(request.Password, superAdmin.PasswordHash))
            {
                _logger.LogWarning("Failed super admin login attempt for {Email} from {Ip}", request.Email, ipAddress);
                throw new UnauthorizedException("Invalid email or password.");
            }

            if (!superAdmin.IsActive)
                throw new UnauthorizedException("Account is deactivated.");

            superAdmin.LastLoginAt = DateTime.UtcNow;
            await _tenantRepository.UpdateSuperAdminAsync(superAdmin, cancellationToken);
            await _unitOfWork.SaveMasterChangesAsync(cancellationToken);

            var (accessToken, accessExpires) = _jwtHelper.GenerateAccessToken(
                superAdmin.Id,
                superAdmin.Email,
                null,
                null,
                [AppConstants.Roles.SuperAdmin],
                isSuperAdmin: true);

            var (refreshToken, refreshExpires) = _jwtHelper.GenerateRefreshToken();

            _logger.LogInformation("Super admin {Email} logged in from {Ip}", request.Email, ipAddress);

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = $"sa:{superAdmin.Id}:{refreshToken}",
                AccessTokenExpiresAt = accessExpires,
                RefreshTokenExpiresAt = refreshExpires,
                User = MapSuperAdmin(superAdmin)
            };
        }

        if (!_tenantContext.IsResolved || string.IsNullOrEmpty(_tenantContext.SchemaName))
            throw new AppException("Tenant header 'X-Tenant-ID' is required for school user login.", 400);

        var user = await _unitOfWork.Users.GetByEmailWithRolesAsync(request.Email, cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("Failed login attempt for unknown user {Email} in tenant {Tenant}",
                request.Email, _tenantContext.TenantSlug);
            throw new UnauthorizedException("Invalid email or password.");
        }

        if (user.LockoutEndAt.HasValue && user.LockoutEndAt > DateTime.UtcNow)
        {
            _logger.LogWarning("Locked-out user {Email} attempted login in tenant {Tenant}",
                request.Email, _tenantContext.TenantSlug);
            throw new UnauthorizedException("Account is temporarily locked. Please try again later.");
        }

        if (!PasswordHelper.VerifyPassword(request.Password, user.Password))
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= 5)
            {
                user.LockoutEndAt = DateTime.UtcNow.AddMinutes(15);
                user.FailedLoginAttempts = 0;
                _logger.LogWarning("User {Email} locked out in tenant {Tenant}", request.Email, _tenantContext.TenantSlug);
            }

            await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveTenantChangesAsync(cancellationToken);

            _logger.LogWarning("Failed login attempt for {Email} in tenant {Tenant} from {Ip}",
                request.Email, _tenantContext.TenantSlug, ipAddress);
            throw new UnauthorizedException("Invalid email or password.");
        }

        if (!user.Active)
            throw new UnauthorizedException("Account is deactivated.");

        user.FailedLoginAttempts = 0;
        user.LockoutEndAt = null;
        user.LastLogin = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);

        var roles = user.UserRoles.Select(ur => ur.Role.Prefix).ToList();
        var (token, tokenExpires) = _jwtHelper.GenerateAccessToken(
            user.Id,
            user.Email,
            _tenantContext.TenantId,
            _tenantContext.SchemaName,
            roles);

        var (refresh, refreshExp) = _jwtHelper.GenerateRefreshToken();
        await _unitOfWork.Users.AddRefreshTokenAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refresh,
            ExpiresAt = refreshExp,
            CreatedByIp = ipAddress,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.Users.AddLoginLogAsync(new LoginLog
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Role = roles.FirstOrDefault() ?? string.Empty,
            Ip = ipAddress ?? "unknown",
            Browser = ParseBrowser(userAgent),
            Platform = ParsePlatform(userAgent),
            Timestamp = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveTenantChangesAsync(cancellationToken);

        _logger.LogInformation("User {Email} logged in to tenant {Tenant} from {Ip}",
            request.Email, _tenantContext.TenantSlug, ipAddress);

        return new LoginResponseDto
        {
            AccessToken = token,
            RefreshToken = refresh,
            AccessTokenExpiresAt = tokenExpires,
            RefreshTokenExpiresAt = refreshExp,
            User = MapUser(user, roles)
        };
    }

    public async Task<UserProfileDto> RegisterAsync(
        RegisterRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.IsResolved || string.IsNullOrEmpty(_tenantContext.SchemaName))
            throw new AppException("Tenant header 'X-Tenant-ID' is required for registration.", 400);

        var tenant = await _tenantRepository.GetByIdAsync(_tenantContext.TenantId!.Value, cancellationToken)
            ?? throw new NotFoundException("Tenant not found.");

        var settings = tenant.GetSettings();
        if (!settings.Features.AllowSelfRegistration)
            throw new ForbiddenException("Self-registration is disabled for this school.");

        var userCount = await _unitOfWork.Users.GetUserCountAsync(cancellationToken);
        if (userCount >= tenant.MaxUsers)
            throw new AppException("Maximum user limit reached for this school.", 403);

        if (await _unitOfWork.Users.EmailExistsAsync(request.Email, cancellationToken))
            throw new ConflictException("Email is already registered.");

        if (await _unitOfWork.Users.UsernameExistsAsync(request.Username, cancellationToken))
            throw new ConflictException("Username is already taken.");

        var role = await _unitOfWork.Users.GetRoleByNameAsync(request.Role, cancellationToken)
            ?? throw new AppException($"Role '{request.Role}' does not exist.", 400);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.ToLowerInvariant(),
            Username = request.Username.ToLowerInvariant(),
            Password = PasswordHelper.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Mobileno = request.Mobileno,
            Active = true,
            IsEmailVerified = !settings.Features.RequireEmailVerification,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.Users.AddUserRoleAsync(new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id
        }, cancellationToken);
        await _unitOfWork.SaveTenantChangesAsync(cancellationToken);

        _logger.LogInformation("User {Email} registered in tenant {Tenant} with role {Role}",
            user.Email, _tenantContext.TenantSlug, role.Prefix);

        return MapUser(user, [role.Prefix]);
    }

    public async Task<LoginResponseDto> RefreshTokenAsync(
        RefreshTokenRequestDto request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        if (request.RefreshToken.StartsWith("sa:", StringComparison.Ordinal))
            throw new UnauthorizedException("Super admin sessions must re-authenticate. Refresh is not supported.");

        if (!_tenantContext.IsResolved)
            throw new AppException("Tenant header 'X-Tenant-ID' is required.", 400);

        var existing = await _unitOfWork.Users.GetRefreshTokenAsync(request.RefreshToken, cancellationToken)
            ?? throw new UnauthorizedException("Invalid refresh token.");

        if (!existing.IsActive)
            throw new UnauthorizedException("Refresh token is expired or revoked.");

        var user = existing.User;
        var roles = user.UserRoles.Select(ur => ur.Role.Prefix).ToList();

        var (newRefresh, newRefreshExp) = _jwtHelper.GenerateRefreshToken();
        existing.IsRevoked = true;
        existing.RevokedAt = DateTime.UtcNow;
        existing.ReplacedByToken = newRefresh;
        await _unitOfWork.Users.UpdateRefreshTokenAsync(existing, cancellationToken);

        await _unitOfWork.Users.AddRefreshTokenAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = newRefresh,
            ExpiresAt = newRefreshExp,
            CreatedByIp = ipAddress,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        var (accessToken, accessExpires) = _jwtHelper.GenerateAccessToken(
            user.Id,
            user.Email,
            _tenantContext.TenantId,
            _tenantContext.SchemaName,
            roles);

        await _unitOfWork.SaveTenantChangesAsync(cancellationToken);

        _logger.LogInformation("Refresh token rotated for user {Email} in tenant {Tenant}",
            user.Email, _tenantContext.TenantSlug);

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = newRefresh,
            AccessTokenExpiresAt = accessExpires,
            RefreshTokenExpiresAt = newRefreshExp,
            User = MapUser(user, roles)
        };
    }

    public async Task RevokeTokenAsync(RevokeTokenRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.RefreshToken.StartsWith("sa:", StringComparison.Ordinal))
        {
            _logger.LogInformation("Super admin logout");
            return;
        }

        if (!_tenantContext.IsResolved)
            throw new AppException("Tenant header 'X-Tenant-ID' is required.", 400);

        var existing = await _unitOfWork.Users.GetRefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (existing is null)
            return;

        existing.IsRevoked = true;
        existing.RevokedAt = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateRefreshTokenAsync(existing, cancellationToken);
        await _unitOfWork.SaveTenantChangesAsync(cancellationToken);

        _logger.LogInformation("Refresh token revoked for user {UserId} in tenant {Tenant}",
            existing.UserId, _tenantContext.TenantSlug);
    }

    public async Task<UserProfileDto> GetCurrentUserAsync(
        Guid userId,
        bool isSuperAdmin,
        CancellationToken cancellationToken = default)
    {
        if (isSuperAdmin)
        {
            var admin = await _tenantRepository.GetSuperAdminByIdAsync(userId, cancellationToken)
                ?? throw new NotFoundException("Super admin not found.");
            return MapSuperAdmin(admin);
        }

        var user = await _unitOfWork.Users.GetByIdWithRolesAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        var roles = user.UserRoles.Select(ur => ur.Role.Prefix).ToList();
        return MapUser(user, roles);
    }

    public async Task<UserProfileDto> UpdateProfileAsync(
        Guid userId,
        UpdateProfileRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (_tenantContext.IsSuperAdmin)
            throw new AppException("Super admin profile updates are not supported via this endpoint.", 400);

        var user = await _unitOfWork.Users.GetByIdWithRolesAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        if (!string.IsNullOrWhiteSpace(request.FirstName))
            user.FirstName = request.FirstName;
        if (!string.IsNullOrWhiteSpace(request.LastName))
            user.LastName = request.LastName;
        if (request.Mobileno is not null)
            user.Mobileno = request.Mobileno;
        if (request.Photo is not null)
            user.Photo = request.Photo;

        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveTenantChangesAsync(cancellationToken);

        var roles = user.UserRoles.Select(ur => ur.Role.Prefix).ToList();
        return MapUser(user, roles);
    }

    private UserProfileDto MapUser(User user, IReadOnlyList<string> roles) => new()
    {
        Id = user.Id,
        Email = user.Email,
        Username = user.Username,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Mobileno = user.Mobileno,
        Photo = user.Photo,
        Active = user.Active,
        IsEmailVerified = user.IsEmailVerified,
        LastLogin = user.LastLogin,
        Roles = roles,
        TenantId = _tenantContext.TenantId,
        TenantSlug = _tenantContext.TenantSlug,
        IsSuperAdmin = false
    };

    private static UserProfileDto MapSuperAdmin(DAL.Entities.Master.SuperAdmin admin) => new()
    {
        Id = admin.Id,
        Email = admin.Email,
        Username = admin.Username,
        FirstName = admin.FirstName,
        LastName = admin.LastName,
        Active = admin.IsActive,
        LastLogin = admin.LastLoginAt,
        Roles = [AppConstants.Roles.SuperAdmin],
        IsSuperAdmin = true
    };

    private static string? ParseBrowser(string? ua)
    {
        if (string.IsNullOrWhiteSpace(ua)) return null;
        if (ua.Contains("Edg/", StringComparison.OrdinalIgnoreCase)) return "Edge";
        if (ua.Contains("Chrome/", StringComparison.OrdinalIgnoreCase)) return "Chrome";
        if (ua.Contains("Firefox/", StringComparison.OrdinalIgnoreCase)) return "Firefox";
        if (ua.Contains("Safari/", StringComparison.OrdinalIgnoreCase) && !ua.Contains("Chrome/", StringComparison.OrdinalIgnoreCase)) return "Safari";
        if (ua.Contains("MSIE", StringComparison.OrdinalIgnoreCase) || ua.Contains("Trident/", StringComparison.OrdinalIgnoreCase)) return "IE";
        return "Other";
    }

    private static string? ParsePlatform(string? ua)
    {
        if (string.IsNullOrWhiteSpace(ua)) return null;
        if (ua.Contains("Android", StringComparison.OrdinalIgnoreCase)) return "Android";
        if (ua.Contains("iPhone", StringComparison.OrdinalIgnoreCase) || ua.Contains("iPad", StringComparison.OrdinalIgnoreCase)) return "iOS";
        if (ua.Contains("Windows", StringComparison.OrdinalIgnoreCase)) return "Windows";
        if (ua.Contains("Mac OS", StringComparison.OrdinalIgnoreCase) || ua.Contains("Macintosh", StringComparison.OrdinalIgnoreCase)) return "macOS";
        if (ua.Contains("Linux", StringComparison.OrdinalIgnoreCase)) return "Linux";
        return "Other";
    }
}
