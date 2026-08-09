using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.Settings;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class UserLoginLogService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http) : IUserLoginLogService
{
    public async Task<LoginLogListDto> GetAsync(
        string? type,
        string? search,
        int page,
        int pageSize,
        string? export,
        CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        Manage();
        page = page < 1 ? 1 : page;
        pageSize = string.IsNullOrWhiteSpace(export) ? (pageSize is < 1 or > 200 ? 20 : pageSize) : 10_000;

        var (items, total) = await uow.LoginLogs.SearchAsync(type, search, page, pageSize, cancellationToken);
        var data = items.Select(l => new LoginLogItemDto
        {
            Id = l.Id,
            UserId = l.UserId,
            UserName = l.User is null ? string.Empty : $"{l.User.FirstName} {l.User.LastName}".Trim(),
            Role = l.Role,
            IpAddress = l.Ip,
            Browser = l.Browser,
            Platform = l.Platform,
            LoginDateTime = l.Timestamp
        }).ToList();

        return new LoginLogListDto
        {
            Data = data,
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize)
        };
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        Manage();
        await uow.LoginLogs.ClearAsync(cancellationToken);
    }

    public static string ToCsv(IEnumerable<LoginLogItemDto> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine("UserName,Role,IpAddress,Browser,Platform,LoginDateTime");
        foreach (var r in rows)
        {
            sb.AppendLine(string.Join(',',
                Csv(r.UserName), Csv(r.Role), Csv(r.IpAddress), Csv(r.Browser), Csv(r.Platform),
                Csv(r.LoginDateTime.ToString("O"))));
        }
        return sb.ToString();
    }

    private static string Csv(string? value)
    {
        value ??= string.Empty;
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }

    private async Task Ready(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(tenant.SchemaName))
            throw new AppException("X-Tenant-ID header is required.", 400);
        await provisioner.EnsureSettingsModuleAsync(tenant.SchemaName!, ct);
    }

    private HashSet<string> Roles() =>
        http.HttpContext?.User.FindAll("role").Concat(http.HttpContext.User.FindAll(ClaimTypes.Role))
            .Select(x => x.Value).ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];

    private void Manage()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin))
            throw new ForbiddenException("Only Super Admin or School Admin can access login logs.");
    }
}
