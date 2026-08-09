using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.StudentAccounting;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class FeesReminderService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http) : IFeesReminderService
{
    public async Task<IReadOnlyList<FeesReminderResponseDto>> GetAllAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        return (await uow.FeesReminders.GetAllAsync(ct)).Select(Map).ToList();
    }

    public async Task<FeesReminderResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var x = await uow.FeesReminders.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Fees reminder '{id}' not found.");
        return Map(x);
    }

    public async Task<FeesReminderResponseDto> CreateAsync(CreateFeesReminderDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        if (string.IsNullOrWhiteSpace(dto.Frequency))
            throw new AppException("Frequency is required.", 400);
        if (dto.Days < 0)
            throw new AppException("Days cannot be negative.", 400);

        var x = new FeesReminder
        {
            Id = Guid.NewGuid(),
            Frequency = dto.Frequency.Trim(),
            Days = dto.Days,
            Message = dto.Message,
            DltTemplateId = dto.DltTemplateId,
            NotifyStudent = dto.NotifyStudent,
            NotifyGuardian = dto.NotifyGuardian,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await uow.FeesReminders.AddAsync(x, ct);
        await uow.SaveTenantChangesAsync(ct);
        return Map(x);
    }

    public async Task<FeesReminderResponseDto> UpdateAsync(Guid id, UpdateFeesReminderDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var x = await uow.FeesReminders.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Fees reminder '{id}' not found.");
        if (string.IsNullOrWhiteSpace(dto.Frequency))
            throw new AppException("Frequency is required.", 400);

        x.Frequency = dto.Frequency.Trim();
        x.Days = dto.Days;
        x.Message = dto.Message;
        x.DltTemplateId = dto.DltTemplateId;
        x.NotifyStudent = dto.NotifyStudent;
        x.NotifyGuardian = dto.NotifyGuardian;
        if (dto.IsActive.HasValue) x.IsActive = dto.IsActive.Value;
        x.UpdatedAt = DateTime.UtcNow;
        await uow.FeesReminders.UpdateAsync(x, ct);
        await uow.SaveTenantChangesAsync(ct);
        return Map(x);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var x = await uow.FeesReminders.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Fees reminder '{id}' not found.");
        await uow.FeesReminders.DeleteAsync(x, ct);
        await uow.SaveTenantChangesAsync(ct);
    }

    private static FeesReminderResponseDto Map(FeesReminder x) => new()
    {
        Id = x.Id,
        Frequency = x.Frequency,
        Days = x.Days,
        Message = x.Message,
        DltTemplateId = x.DltTemplateId,
        NotifyStudent = x.NotifyStudent,
        NotifyGuardian = x.NotifyGuardian,
        IsActive = x.IsActive,
        CreatedAt = x.CreatedAt
    };

    private async Task Ready(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(tenant.SchemaName))
            throw new AppException("X-Tenant-ID header is required.", 400);
        await provisioner.EnsureStudentAndOfficeAccountingModuleAsync(tenant.SchemaName!, ct);
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
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin) && !r.Contains(AppConstants.Roles.Accountant))
            throw new ForbiddenException("Only Super Admin, School Admin, or Accountant can manage fees reminders.");
    }
}
