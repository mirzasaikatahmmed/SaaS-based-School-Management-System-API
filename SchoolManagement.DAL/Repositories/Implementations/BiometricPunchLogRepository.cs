using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class BiometricPunchLogRepository(TenantDbContext context) : IBiometricPunchLogRepository
{
    public async Task<(IReadOnlyList<BiometricPunchLog> Items, int TotalCount)> GetFilteredAsync(
        DateTime? from, DateTime? to, Guid? deviceId, string? kind,
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var q = context.BiometricPunchLogs
            .Include(p => p.Device)
            .Include(p => p.Student)
            .Include(p => p.Employee)
            .AsQueryable();

        if (from.HasValue) q = q.Where(p => p.PunchTime.Date >= from.Value.Date);
        if (to.HasValue) q = q.Where(p => p.PunchTime.Date <= to.Value.Date);
        if (deviceId.HasValue) q = q.Where(p => p.DeviceId == deviceId.Value);
        if (!string.IsNullOrWhiteSpace(kind)) q = q.Where(p => p.PunchKind == kind);

        var total = await q.CountAsync(cancellationToken);
        var page2 = page < 1 ? 1 : page;
        var size = pageSize is < 1 or > 500 ? 50 : pageSize;
        var items = await q.OrderByDescending(p => p.PunchTime)
            .Skip((page2 - 1) * size).Take(size)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<BiometricPunchLog> AddAsync(BiometricPunchLog entity, CancellationToken cancellationToken = default)
    {
        await context.BiometricPunchLogs.AddAsync(entity, cancellationToken);
        return entity;
    }
}
