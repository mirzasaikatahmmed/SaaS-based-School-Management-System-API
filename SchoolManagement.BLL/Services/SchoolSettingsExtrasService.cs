using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.BLL.DTOs.Settings;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Implementations;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class SchoolSettingsExtrasService(
    IUnitOfWork uow,
    ITenantContext tenantContext,
    ITenantDbContextFactory tenantDbFactory,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http) : ISchoolSettingsExtrasService
{
    public async Task<AttendanceTypeDto> GetAttendanceTypeAsync(string slug, CancellationToken cancellationToken = default)
    {
        var tenant = await ResolveTenantAsync(slug, cancellationToken);
        await using var db = tenantDbFactory.Create(tenant.SchemaName!);
        await provisioner.EnsureSettingsModuleAsync(tenant.SchemaName!, cancellationToken);
        var settings = await new SchoolSettingsRepository(db).GetOrCreateAsync(cancellationToken);
        return new AttendanceTypeDto { AttendanceType = settings.AttendanceType };
    }

    public async Task<AttendanceTypeDto> UpdateAttendanceTypeAsync(string slug, UpdateAttendanceTypeDto dto, CancellationToken cancellationToken = default)
    {
        if (!AttendanceTypes.IsValid(dto.AttendanceType))
            throw new AppException("AttendanceType must be DayWise or SubjectWise.", 400);

        var tenant = await ResolveTenantAsync(slug, cancellationToken);
        await using var db = tenantDbFactory.Create(tenant.SchemaName!);
        await provisioner.EnsureSettingsModuleAsync(tenant.SchemaName!, cancellationToken);
        var repo = new SchoolSettingsRepository(db);
        var settings = await repo.GetOrCreateAsync(cancellationToken);
        settings.AttendanceType = AttendanceTypes.All.First(x =>
            x.Equals(dto.AttendanceType.Trim(), StringComparison.OrdinalIgnoreCase));
        settings.UpdatedAt = DateTime.UtcNow;
        await repo.UpdateAsync(settings, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return new AttendanceTypeDto { AttendanceType = settings.AttendanceType };
    }

    public async Task<AccountingLinksDto> GetAccountingLinksAsync(string slug, CancellationToken cancellationToken = default)
    {
        var tenant = await ResolveTenantAsync(slug, cancellationToken);
        await using var db = tenantDbFactory.Create(tenant.SchemaName!);
        await provisioner.EnsureSettingsModuleAsync(tenant.SchemaName!, cancellationToken);
        var settings = await new SchoolSettingsRepository(db).GetOrCreateAsync(cancellationToken);
        return new AccountingLinksDto
        {
            IsEnabled = settings.AccountingLinksEnabled,
            DefaultDepositAccountId = settings.DefaultDepositAccountId,
            DefaultExpenseAccountId = settings.DefaultExpenseAccountId
        };
    }

    public async Task<AccountingLinksDto> UpdateAccountingLinksAsync(string slug, AccountingLinksDto dto, CancellationToken cancellationToken = default)
    {
        var tenant = await ResolveTenantAsync(slug, cancellationToken);
        await using var db = tenantDbFactory.Create(tenant.SchemaName!);
        await provisioner.EnsureSettingsModuleAsync(tenant.SchemaName!, cancellationToken);
        var repo = new SchoolSettingsRepository(db);
        var settings = await repo.GetOrCreateAsync(cancellationToken);

        if (dto.DefaultDepositAccountId.HasValue &&
            !await db.AccountingAccounts.AnyAsync(a => a.Id == dto.DefaultDepositAccountId.Value, cancellationToken))
            throw new NotFoundException("Default deposit account not found.");
        if (dto.DefaultExpenseAccountId.HasValue &&
            !await db.AccountingAccounts.AnyAsync(a => a.Id == dto.DefaultExpenseAccountId.Value, cancellationToken))
            throw new NotFoundException("Default expense account not found.");

        settings.AccountingLinksEnabled = dto.IsEnabled;
        settings.DefaultDepositAccountId = dto.DefaultDepositAccountId;
        settings.DefaultExpenseAccountId = dto.DefaultExpenseAccountId;
        settings.UpdatedAt = DateTime.UtcNow;
        await repo.UpdateAsync(settings, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return new AccountingLinksDto
        {
            IsEnabled = settings.AccountingLinksEnabled,
            DefaultDepositAccountId = settings.DefaultDepositAccountId,
            DefaultExpenseAccountId = settings.DefaultExpenseAccountId
        };
    }

    private async Task<DAL.Entities.Master.Tenant> ResolveTenantAsync(string slug, CancellationToken ct)
    {
        var tenant = await uow.Tenants.GetBySlugAsync(slug.Trim().ToLowerInvariant(), ct)
            ?? throw new NotFoundException($"School '{slug}' not found.");
        EnsureCanAccessSchool(tenant.Slug);
        return tenant;
    }

    private void EnsureCanAccessSchool(string slug)
    {
        if (Roles().Contains(AppConstants.Roles.SuperAdmin)) return;
        if (Roles().Contains(AppConstants.Roles.Admin))
        {
            if (string.IsNullOrEmpty(tenantContext.TenantSlug))
                throw new ForbiddenException("X-Tenant-ID header is required for school admin access.");
            if (!string.Equals(tenantContext.TenantSlug, slug, StringComparison.OrdinalIgnoreCase))
                throw new ForbiddenException("You can only access your own school's settings.");
            return;
        }
        throw new ForbiddenException("You do not have permission to access school settings.");
    }

    private HashSet<string> Roles()
    {
        var p = http.HttpContext?.User;
        if (p is null) return [];
        return p.FindAll("role").Concat(p.FindAll(ClaimTypes.Role)).Select(x => x.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}
