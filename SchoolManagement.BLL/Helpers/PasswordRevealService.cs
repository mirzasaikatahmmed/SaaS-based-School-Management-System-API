using Microsoft.AspNetCore.DataProtection;

namespace SchoolManagement.BLL.Helpers;

/// <summary>
/// Stores a reversible copy of user passwords so Login Credential reports can show
/// student/parent passwords (login still uses bcrypt <c>User.Password</c>).
/// </summary>
public interface IPasswordRevealService
{
    string Protect(string plainPassword);
    string? Unprotect(string? protectedValue);
    void Apply(DAL.Entities.Tenant.User user, string plainPassword);
}

public class PasswordRevealService(IDataProtectionProvider provider) : IPasswordRevealService
{
    private readonly IDataProtector _protector = provider.CreateProtector("SchoolManagement.UserPasswordReveal.v1");

    public string Protect(string plainPassword) => _protector.Protect(plainPassword);

    public string? Unprotect(string? protectedValue)
    {
        if (string.IsNullOrWhiteSpace(protectedValue)) return null;
        try { return _protector.Unprotect(protectedValue); }
        catch { return null; }
    }

    public void Apply(DAL.Entities.Tenant.User user, string plainPassword)
    {
        user.Password = PasswordHelper.HashPassword(plainPassword);
        user.PasswordRevealEncrypted = Protect(plainPassword);
        user.UpdatedAt = DateTime.UtcNow;
    }
}
