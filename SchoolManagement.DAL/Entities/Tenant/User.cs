namespace SchoolManagement.DAL.Entities.Tenant;

/// <summary>
/// Tenant-scoped user profile. Column names follow ahskbera_main conventions
/// (snake_case: password, mobileno, photo, active, last_login, created_at, updated_at).
/// </summary>
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;

    /// <summary>Mapped to column "password" — stores bcrypt hash ($2y$ / $2a$).</summary>
    public string Password { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    /// <summary>Mapped to column "mobileno".</summary>
    public string? Mobileno { get; set; }

    /// <summary>Mapped to column "photo" (relative path / object key).</summary>
    public string? Photo { get; set; }

    /// <summary>Mapped to column "active" (1=active, 0=deactivate) — ahskbera login_credential.active.</summary>
    public bool Active { get; set; } = true;

    public bool IsEmailVerified { get; set; } = false;

    /// <summary>Mapped to column "last_login".</summary>
    public DateTime? LastLogin { get; set; }

    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockoutEndAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<LoginLog> LoginLogs { get; set; } = new List<LoginLog>();
}

public class Role
{
    public Guid Id { get; set; }

    /// <summary>Display name, e.g. "Teacher" — ahskbera roles.name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>JWT / authorize key, e.g. "teacher" — ahskbera roles.prefix.</summary>
    public string Prefix { get; set; } = string.Empty;

    /// <summary>ahskbera roles.is_system ("1"/"0").</summary>
    public bool IsSystem { get; set; } = true;

    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

public class UserRole
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }

    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}

public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public string? CreatedByIp { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;

    public User User { get; set; } = null!;
}

/// <summary>Aligned with ahskbera_main.login_log.</summary>
public class LoginLog
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Ip { get; set; } = string.Empty;
    public string? Browser { get; set; }
    public string? Platform { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
