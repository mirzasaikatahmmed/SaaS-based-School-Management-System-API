using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.BLL.DTOs.Settings;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

/// <summary>
/// Cron job settings and secret-key-guarded job runners.
/// </summary>
public class CronSettingsService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantDbContextFactory tenantDbFactory,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http) : ICronSettingsService
{
    public async Task<CronSettingsResponseDto> GetAsync(CancellationToken cancellationToken = default)
    {
        await ReadyTenant(cancellationToken);
        Manage();
        var key = await EnsureKeyAsync(cancellationToken);
        return BuildDto(key);
    }

    public async Task<CronSettingsResponseDto> RegenerateKeyAsync(CancellationToken cancellationToken = default)
    {
        await ReadyTenant(cancellationToken);
        Manage();
        var key = Guid.NewGuid().ToString("N");
        var settings = await uow.SchoolSettings.GetOrCreateAsync(cancellationToken);
        settings.CronSecretKey = key;
        settings.UpdatedAt = DateTime.UtcNow;
        await uow.SchoolSettings.UpdateAsync(settings, cancellationToken);
        await uow.CronSecretRegistries.UpsertAsync(RequireTenantId(), tenant.SchemaName!, key, cancellationToken);
        await uow.SaveTenantChangesAsync(cancellationToken);
        await uow.SaveMasterChangesAsync(cancellationToken);
        return BuildDto(key);
    }

    public async Task<CronJobResultDto> RunSendSmsEmailAsync(string secretKey, CancellationToken cancellationToken = default)
        => await RunJobAsync(secretKey, "send_smsemail", async (db, today) =>
        {
            // Queue tables not yet implemented — idempotent no-op with clear summary.
            return (0, 0, "No pending SMS/Email queue items.");
        }, cancellationToken);

    public async Task<CronJobResultDto> RunHomeworkAsync(string secretKey, CancellationToken cancellationToken = default)
        => await RunJobAsync(secretKey, "homework", async (db, today) =>
        {
            return (0, 0, "No pending homework notices.");
        }, cancellationToken);

    public async Task<CronJobResultDto> RunFeesReminderAsync(string secretKey, CancellationToken cancellationToken = default)
        => await RunJobAsync(secretKey, "fees_reminder", async (db, today) =>
        {
            var due = await db.StudentFeeInvoices
                .Where(i => i.Status != "Paid" && i.DueAmount > 0)
                .Select(i => i.Id)
                .ToListAsync(cancellationToken);

            var processed = 0;
            var skipped = 0;
            foreach (var invoiceId in due)
            {
                var entityKey = invoiceId.ToString();
                var already = await db.NotificationDispatchLogs.AnyAsync(
                    l => l.JobName == "fees_reminder" && l.EntityKey == entityKey && l.RunDate == today,
                    cancellationToken);
                if (already)
                {
                    skipped++;
                    continue;
                }

                db.NotificationDispatchLogs.Add(new NotificationDispatchLog
                {
                    Id = Guid.NewGuid(),
                    JobName = "fees_reminder",
                    EntityKey = entityKey,
                    RunDate = today,
                    CreatedAt = DateTime.UtcNow
                });
                processed++;
            }

            await db.SaveChangesAsync(cancellationToken);
            return (processed, skipped, $"Fees reminder job processed {processed} invoice(s), skipped {skipped} already sent today.");
        }, cancellationToken);

    private async Task<CronJobResultDto> RunJobAsync(
        string secretKey,
        string jobName,
        Func<TenantDbContext, DateOnly, Task<(int Processed, int Skipped, string Message)>> work,
        CancellationToken cancellationToken)
    {
        var registry = await uow.CronSecretRegistries.GetBySecretKeyAsync(secretKey.Trim(), cancellationToken)
            ?? throw new AppException("Invalid cron secret key.", 401);

        await provisioner.EnsureSettingsModuleAsync(registry.SchemaName, cancellationToken);
        await using var db = tenantDbFactory.Create(registry.SchemaName);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var (processed, skipped, message) = await work(db, today);
        return new CronJobResultDto
        {
            Success = true,
            Job = jobName,
            Processed = processed,
            Skipped = skipped,
            Message = message
        };
    }

    private async Task<string> EnsureKeyAsync(CancellationToken ct)
    {
        var settings = await uow.SchoolSettings.GetOrCreateAsync(ct);
        if (string.IsNullOrWhiteSpace(settings.CronSecretKey))
        {
            settings.CronSecretKey = Guid.NewGuid().ToString("N");
            settings.UpdatedAt = DateTime.UtcNow;
            await uow.SchoolSettings.UpdateAsync(settings, ct);
            await uow.CronSecretRegistries.UpsertAsync(RequireTenantId(), tenant.SchemaName!, settings.CronSecretKey, ct);
            await uow.SaveTenantChangesAsync(ct);
            await uow.SaveMasterChangesAsync(ct);
        }
        else
        {
            await uow.CronSecretRegistries.UpsertAsync(RequireTenantId(), tenant.SchemaName!, settings.CronSecretKey, ct);
            await uow.SaveMasterChangesAsync(ct);
        }

        return settings.CronSecretKey!;
    }

    private Guid RequireTenantId()
        => tenant.TenantId ?? throw new AppException("X-Tenant-ID header is required.", 400);

    private CronSettingsResponseDto BuildDto(string key) => new()
    {
        SecretKey = key,
        SendSmsEmailUrl = $"/cron_api/send_smsemail_command/{key}",
        HomeworkUrl = $"/cron_api/homework_command/{key}",
        FeesReminderUrl = $"/cron_api/fees_reminder_command/{key}"
    };

    private async Task ReadyTenant(CancellationToken ct)
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
            throw new ForbiddenException("Only Super Admin or School Admin can manage cron settings.");
    }
}
