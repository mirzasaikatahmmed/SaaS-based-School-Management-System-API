using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class BiometricPunchLogRepository(TenantDbContext context) : IBiometricPunchLogRepository
{
    public Task<(IReadOnlyList<BiometricPunchLog> Items, int TotalCount)> GetFilteredAsync(
        DateTime? from, DateTime? to, Guid? deviceId, string? kind,
        int page, int pageSize, CancellationToken cancellationToken = default)
        => GetFilteredAsync(new BiometricPunchLogFilter
        {
            From = from,
            To = to,
            DeviceId = deviceId,
            Kind = kind,
            Page = page,
            PageSize = pageSize
        }, cancellationToken);

    public async Task<(IReadOnlyList<BiometricPunchLog> Items, int TotalCount)> GetFilteredAsync(
        BiometricPunchLogFilter filter, CancellationToken cancellationToken = default)
    {
        var q = context.BiometricPunchLogs
            .Include(p => p.Device)
            .Include(p => p.Student).ThenInclude(s => s!.Class)
            .Include(p => p.Student).ThenInclude(s => s!.Section)
            .Include(p => p.Employee)
            .AsQueryable();

        // Inclusive datetime window on PunchTime (every fingerprint hit is a log row).
        if (filter.From.HasValue)
        {
            var from = filter.From.Value;
            q = q.Where(p => p.PunchTime >= from);
        }
        if (filter.To.HasValue)
        {
            var to = filter.To.Value;
            q = q.Where(p => p.PunchTime <= to);
        }
        if (filter.DeviceId.HasValue) q = q.Where(p => p.DeviceId == filter.DeviceId.Value);
        if (!string.IsNullOrWhiteSpace(filter.Kind)) q = q.Where(p => p.PunchKind == filter.Kind);
        if (filter.StudentId.HasValue) q = q.Where(p => p.StudentId == filter.StudentId.Value);
        if (filter.EmployeeId.HasValue) q = q.Where(p => p.EmployeeId == filter.EmployeeId.Value);
        if (!string.IsNullOrWhiteSpace(filter.DevicePin))
            q = q.Where(p => p.DevicePin == filter.DevicePin.Trim());

        if (!string.IsNullOrWhiteSpace(filter.Role))
        {
            var role = filter.Role.Trim();
            if (role.Equals("Student", StringComparison.OrdinalIgnoreCase))
            {
                q = q.Where(p => p.StudentId != null);
                if (filter.ClassId.HasValue) q = q.Where(p => p.Student != null && p.Student.ClassId == filter.ClassId.Value);
                if (filter.SectionId.HasValue) q = q.Where(p => p.Student != null && p.Student.SectionId == filter.SectionId.Value);
            }
            else
            {
                q = q.Where(p => p.EmployeeId != null && p.Employee != null
                                 && p.Employee.Role.ToLower() == role.ToLower());
            }
        }
        else
        {
            if (filter.ClassId.HasValue) q = q.Where(p => p.Student != null && p.Student.ClassId == filter.ClassId.Value);
            if (filter.SectionId.HasValue) q = q.Where(p => p.Student != null && p.Student.SectionId == filter.SectionId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            q = q.Where(p =>
                p.DevicePin.ToLower().Contains(s) ||
                p.DeviceSn.ToLower().Contains(s) ||
                (p.Device != null && p.Device.Name.ToLower().Contains(s)) ||
                (p.Employee != null && p.Employee.Name.ToLower().Contains(s)) ||
                (p.Student != null && (
                    p.Student.FirstName.ToLower().Contains(s) ||
                    (p.Student.LastName != null && p.Student.LastName.ToLower().Contains(s)) ||
                    p.Student.RegisterNo.ToLower().Contains(s) ||
                    (p.Student.Roll != null && p.Student.Roll.ToLower().Contains(s)))));
        }

        var total = await q.CountAsync(cancellationToken);
        var page = filter.Page < 1 ? 1 : filter.Page;
        var size = filter.PageSize is < 1 or > 5000 ? 50 : filter.PageSize;
        var items = await q.OrderByDescending(p => p.PunchTime).ThenByDescending(p => p.CreatedAt)
            .Skip((page - 1) * size).Take(size)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<BiometricPunchLog> AddAsync(BiometricPunchLog entity, CancellationToken cancellationToken = default)
    {
        await context.BiometricPunchLogs.AddAsync(entity, cancellationToken);
        return entity;
    }
}
