namespace SchoolManagement.BLL.DTOs.Auth;

public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; set; }
    public DateTime RefreshTokenExpiresAt { get; set; }
    public UserProfileDto User { get; set; } = null!;
}

public class RegisterRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    /// <summary>Matches ahskbera mobileno.</summary>
    public string? Mobileno { get; set; }
    /// <summary>Role prefix from ahskbera roles.prefix (e.g. teacher, student).</summary>
    public string Role { get; set; } = "student";
}

public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class RevokeTokenRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class UpdateProfileRequestDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Mobileno { get; set; }
    public string? Photo { get; set; }
}

public class UserProfileDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Mobileno { get; set; }
    public string? Photo { get; set; }
    public bool Active { get; set; }
    public bool IsEmailVerified { get; set; }
    public DateTime? LastLogin { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
    public Guid? TenantId { get; set; }
    public string? TenantSlug { get; set; }
    public bool IsSuperAdmin { get; set; }
}
