using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SchoolManagement.BLL.Settings;
using SchoolManagement.Common.Constants;

namespace SchoolManagement.BLL.Helpers;

public class JwtHelper
{
    private readonly JwtSettings _settings;

    public JwtHelper(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    public (string Token, DateTime ExpiresAt) GenerateAccessToken(
        Guid userId,
        string email,
        Guid? tenantId,
        string? schemaName,
        IEnumerable<string> roles,
        bool isSuperAdmin = false)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpiryMinutes);
        var claims = new List<Claim>
        {
            new(AppConstants.Claims.UserId, userId.ToString()),
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(AppConstants.Claims.IsSuperAdmin, isSuperAdmin.ToString().ToLowerInvariant())
        };

        if (tenantId.HasValue)
            claims.Add(new Claim(AppConstants.Claims.TenantId, tenantId.Value.ToString()));

        if (!string.IsNullOrEmpty(schemaName))
            claims.Add(new Claim(AppConstants.Claims.SchemaName, schemaName));

        foreach (var role in roles)
        {
            claims.Add(new Claim("role", role));
            claims.Add(new Claim(AppConstants.Claims.Roles, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var handler = new JwtSecurityTokenHandler();
        handler.OutboundClaimTypeMap.Clear();
        return (handler.WriteToken(token), expiresAt);
    }

    public (string Token, DateTime ExpiresAt) GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        var token = Convert.ToBase64String(bytes);
        var expiresAt = DateTime.UtcNow.AddDays(_settings.RefreshTokenExpiryDays);
        return (token, expiresAt);
    }
}
