using Microsoft.EntityFrameworkCore;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class UserRepository : IUserRepository
{
    private readonly TenantDbContext _context;

    public UserRepository(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username.ToLowerInvariant(), cancellationToken);
    }

    public async Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailWithRolesAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        return user;
    }

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        user.UpdatedAt = DateTime.UtcNow;
        _context.Users.Update(user);
        return Task.CompletedTask;
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AnyAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);
    }

    public async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AnyAsync(u => u.Username == username.ToLowerInvariant(), cancellationToken);
    }

    public async Task<int> GetUserCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users.CountAsync(cancellationToken);
    }

    public async Task<Role?> GetRoleByNameAsync(string nameOrPrefix, CancellationToken cancellationToken = default)
    {
        var key = nameOrPrefix.Trim().ToLowerInvariant();
        return await _context.Roles
            .FirstOrDefaultAsync(r =>
                r.Prefix == key ||
                r.Name.ToLower() == key ||
                r.Name.Replace(" ", "").ToLower() == key.Replace(" ", ""),
                cancellationToken);
    }

    public async Task AddUserRoleAsync(UserRole userRole, CancellationToken cancellationToken = default)
    {
        await _context.UserRoles.AddAsync(userRole, cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Return prefixes for JWT [Authorize(Roles=...)] — matches ahskbera roles.prefix
        return await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role.Prefix)
            .ToListAsync(cancellationToken);
    }

    public async Task SeedRolesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var (id, name, prefix, isSystem) in AppConstants.Roles.Seed)
        {
            if (prefix == AppConstants.Roles.SuperAdmin)
                continue; // Super Admin lives in master schema only

            if (!await _context.Roles.AnyAsync(r => r.Prefix == prefix, cancellationToken))
            {
                await _context.Roles.AddAsync(new Role
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Prefix = prefix,
                    IsSystem = isSystem,
                    Description = $"{name} role",
                    CreatedAt = DateTime.UtcNow
                }, cancellationToken);
            }
        }
    }

    public async Task AddLoginLogAsync(LoginLog log, CancellationToken cancellationToken = default)
    {
        await _context.LoginLogs.AddAsync(log, cancellationToken);
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .ThenInclude(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }

    public async Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
    }

    public Task UpdateRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        _context.RefreshTokens.Update(refreshToken);
        return Task.CompletedTask;
    }
}
