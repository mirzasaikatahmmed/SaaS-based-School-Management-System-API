using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.Events;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class EventTypeService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http) : IEventTypeService
{
    public async Task<IReadOnlyList<EventTypeDto>> GetAllAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var items = await uow.EventTypes.GetAllAsync(ct);
        var result = new List<EventTypeDto>();
        foreach (var t in items)
        {
            var count = await uow.EventTypes.CountEventsUsingAsync(t.Id, ct);
            result.Add(Map(t, count));
        }
        return result;
    }

    public async Task<EventTypeDto> CreateAsync(CreateEventTypeDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();

        var name = dto.Name.Trim();
        if (await uow.EventTypes.NameExistsAsync(name, null, ct))
            throw new ConflictException($"Event type '{name}' already exists.");

        var entity = new EventType
        {
            Id = Guid.NewGuid(),
            Name = name,
            Icon = dto.Icon?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await uow.EventTypes.AddAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
        return Map(entity, 0);
    }

    public async Task<EventTypeDto> UpdateAsync(Guid id, UpdateEventTypeDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();

        var entity = await uow.EventTypes.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Event type '{id}' not found.");

        var name = dto.Name.Trim();
        if (await uow.EventTypes.NameExistsAsync(name, id, ct))
            throw new ConflictException($"Event type '{name}' already exists.");

        entity.Name = name;
        entity.Icon = dto.Icon?.Trim();
        if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;

        await uow.EventTypes.UpdateAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
        var count = await uow.EventTypes.CountEventsUsingAsync(id, ct);
        return Map(entity, count);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();

        var entity = await uow.EventTypes.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Event type '{id}' not found.");

        var count = await uow.EventTypes.CountEventsUsingAsync(id, ct);
        if (count > 0)
            throw new AppException($"Event type is in use by {count} event(s) and cannot be deleted.", 400);

        await uow.EventTypes.DeleteAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
    }

    private EventTypeDto Map(EventType t, int count) => new()
    {
        Id = t.Id,
        Branch = tenant.TenantName ?? string.Empty,
        Name = t.Name,
        Icon = t.Icon,
        IsActive = t.IsActive,
        EventCount = count,
        CreatedAt = t.CreatedAt
    };

    private async Task Ready(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(tenant.SchemaName))
            throw new AppException("X-Tenant-ID header is required.", 400);
        await provisioner.EnsureGradesAttendanceLibraryEventsModuleAsync(tenant.SchemaName!, ct);
    }

    private HashSet<string> Roles()
    {
        var p = http.HttpContext?.User;
        if (p is null) return [];
        return p.FindAll("role").Concat(p.FindAll(ClaimTypes.Role)).Select(x => x.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private void Manage()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin))
            throw new ForbiddenException("Only Super Admin or School Admin can manage event types.");
    }
}
