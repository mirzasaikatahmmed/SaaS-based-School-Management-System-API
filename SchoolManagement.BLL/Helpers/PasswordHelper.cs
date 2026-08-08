using SchoolManagement.Common.Constants;

namespace SchoolManagement.BLL.Helpers;

public static class PasswordHelper
{
    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, AppConstants.BcryptWorkFactor);
    }

    /// <summary>
    /// Verifies bcrypt hashes including PHP-style $2y$ from ahskbera_main.login_credential.
    /// </summary>
    public static bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            return false;

        // PHP password_hash() uses $2y$; BCrypt.Net expects $2a$ / $2b$
        var normalized = passwordHash.StartsWith("$2y$", StringComparison.Ordinal)
            ? "$2a$" + passwordHash[4..]
            : passwordHash;

        return BCrypt.Net.BCrypt.Verify(password, normalized);
    }
}
