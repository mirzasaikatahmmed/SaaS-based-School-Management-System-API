using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.Auth;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login(
        [FromBody] LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _authService.LoginAsync(request, ip, cancellationToken);
        return Ok(ApiResponse<LoginResponseDto>.Ok(result, "Login successful"));
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> Register(
        [FromBody] RegisterRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(request, cancellationToken);
        return Ok(ApiResponse<UserProfileDto>.Ok(result, "Registration successful"));
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> RefreshToken(
        [FromBody] RefreshTokenRequestDto request,
        CancellationToken cancellationToken)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _authService.RefreshTokenAsync(request, ip, cancellationToken);
        return Ok(ApiResponse<LoginResponseDto>.Ok(result, "Token refreshed"));
    }

    [HttpPost("revoke-token")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> RevokeToken(
        [FromBody] RevokeTokenRequestDto request,
        CancellationToken cancellationToken)
    {
        await _authService.RevokeTokenAsync(request, cancellationToken);
        return Ok(ApiResponse.Ok("Token revoked"));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> Me(CancellationToken cancellationToken)
    {
        var (userId, isSuperAdmin) = GetCurrentUser();
        var result = await _authService.GetCurrentUserAsync(userId, isSuperAdmin, cancellationToken);
        return Ok(ApiResponse<UserProfileDto>.Ok(result, "Profile retrieved"));
    }

    [HttpPut("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> UpdateMe(
        [FromBody] UpdateProfileRequestDto request,
        CancellationToken cancellationToken)
    {
        var (userId, isSuperAdmin) = GetCurrentUser();
        if (isSuperAdmin)
            return BadRequest(ApiResponse<UserProfileDto>.Fail("Super admin profile cannot be updated via this endpoint."));

        var result = await _authService.UpdateProfileAsync(userId, request, cancellationToken);
        return Ok(ApiResponse<UserProfileDto>.Ok(result, "Profile updated"));
    }

    private (Guid UserId, bool IsSuperAdmin) GetCurrentUser()
    {
        var userIdClaim = User.FindFirstValue(AppConstants.Claims.UserId)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(ClaimTypes.Name);

        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("Invalid user identity.");

        var isSuperAdmin = string.Equals(
            User.FindFirstValue(AppConstants.Claims.IsSuperAdmin),
            "true",
            StringComparison.OrdinalIgnoreCase);

        return (userId, isSuperAdmin);
    }
}
