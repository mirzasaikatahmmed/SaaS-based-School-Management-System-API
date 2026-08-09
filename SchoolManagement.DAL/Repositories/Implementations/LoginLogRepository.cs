using Microsoft.EntityFrameworkCore;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class LoginLogRepository(TenantDbContext context) : ILoginLogRepository
{
    private static readonly HashSet<string> StaffRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        AppConstants.Roles.Admin,
        AppConstants.Roles.Teacher,
        AppConstants.Roles.Accountant,
        AppConstants.Roles.Librarian,
        AppConstants.Roles.Receptionist,
        AppConstants.Roles.Staff,
        AppConstants.Roles.Demo
    };

    public async Task<(IReadOnlyList<LoginLog> Items, int Total)> SearchAsync(
        string? type,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var q = context.LoginLogs.Include(l => l.User).AsQueryable();

        var t = type?.Trim().ToLowerInvariant();
        q = t switch
        {
            "student" => q.Where(l => l.Role == AppConstants.Roles.Student),
            "parent" => q.Where(l => l.Role == AppConstants.Roles.Parent),
            "staff" => q.Where(l => StaffRoles.Contains(l.Role)),
            _ => q
        };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            q = q.Where(l =>
                l.User.Username.ToLower().Contains(s) ||
                l.User.Email.ToLower().Contains(s) ||
                l.User.FirstName.ToLower().Contains(s) ||
                l.User.LastName.ToLower().Contains(s) ||
                l.Ip.ToLower().Contains(s) ||
                l.Role.ToLower().Contains(s));
        }

        var total = await q.CountAsync(cancellationToken);
        var items = await q.OrderByDescending(l => l.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        await context.LoginLogs.ExecuteDeleteAsync(cancellationToken);
    }
}
