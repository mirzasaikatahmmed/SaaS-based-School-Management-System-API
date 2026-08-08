using SchoolManagement.BLL.DTOs.Auth;

namespace SchoolManagement.BLL.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, string? ipAddress, CancellationToken cancellationToken = default);
    Task<UserProfileDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);
    Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, string? ipAddress, CancellationToken cancellationToken = default);
    Task RevokeTokenAsync(RevokeTokenRequestDto request, CancellationToken cancellationToken = default);
    Task<UserProfileDto> GetCurrentUserAsync(Guid userId, bool isSuperAdmin, CancellationToken cancellationToken = default);
    Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequestDto request, CancellationToken cancellationToken = default);
}
