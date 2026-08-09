using System.Globalization;
using Microsoft.Extensions.Logging;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.DAL.Entities.Master;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

/// <summary>
/// Handles the ZKTeco K40-H ADMS push-protocol surface under /iclock/*.
/// Every public method swallows exceptions and always answers "OK" (or a valid handshake
/// block) — a broken/slow backend must never make the device retry forever or brick its queue.
/// </summary>
public class ZkTecoAdmsService(
    IUnitOfWork uow,
    ITenantContext tenantContext,
    IBiometricPunchProcessor processor,
    ILogger<ZkTecoAdmsService> logger) : IZkTecoAdmsService
{
    public async Task<string> HandleCdataGetAsync(string serialNumber, string? options, CancellationToken cancellationToken = default)
    {
        try
        {
            var registry = await ResolveTenantAsync(serialNumber, cancellationToken);
            var attStamp = registry?.AttLogStamp ?? "0";
            var opStamp = registry?.OperLogStamp ?? "0";

            if (registry is not null)
            {
                registry.LastSeenAt = DateTime.UtcNow;
                await uow.BiometricDeviceRegistries.UpdateAsync(registry, cancellationToken);
                await uow.SaveMasterChangesAsync(cancellationToken);
                await TouchTenantDeviceLastSeenAsync(serialNumber, cancellationToken);
            }
            else
            {
                logger.LogWarning("ADMS handshake from unregistered device SN={SerialNumber}", serialNumber);
            }

            var lines = new[]
            {
                $"GET OPTION FROM: {serialNumber}",
                $"Stamp={attStamp}",
                $"OpStamp={opStamp}",
                "ErrorDelay=30",
                "Delay=30",
                "TransFlag=1111111111",
                "TransInterval=1",
                "Realtime=1",
                "Encrypt=0"
            };
            return string.Join("\n", lines);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ADMS handshake failed for SN={SerialNumber}", serialNumber);
            return string.Join("\n", $"GET OPTION FROM: {serialNumber}", "Stamp=0", "OpStamp=0", "ErrorDelay=30", "Delay=30");
        }
    }

    public async Task<string> HandleCdataPostAsync(
        string serialNumber, string? table, string? stamp, string body, CancellationToken cancellationToken = default)
    {
        try
        {
            var registry = await ResolveTenantAsync(serialNumber, cancellationToken);
            if (registry is null)
            {
                logger.LogWarning(
                    "ADMS cdata upload from unregistered device SN={SerialNumber}, table={Table}", serialNumber, table);
                return "OK";
            }

            if (string.Equals(table, "ATTLOG", StringComparison.OrdinalIgnoreCase))
            {
                await ProcessAttLogAsync(serialNumber, body, cancellationToken);
                if (!string.IsNullOrWhiteSpace(stamp))
                    registry.AttLogStamp = stamp;
            }
            else if (string.Equals(table, "OPERLOG", StringComparison.OrdinalIgnoreCase))
            {
                // OPERLOG carries admin/operation events (enroll, delete, etc.) — accepted, not processed.
                if (!string.IsNullOrWhiteSpace(stamp))
                    registry.OperLogStamp = stamp;
            }

            registry.LastSeenAt = DateTime.UtcNow;
            await uow.BiometricDeviceRegistries.UpdateAsync(registry, cancellationToken);
            await uow.SaveMasterChangesAsync(cancellationToken);
            await TouchTenantDeviceLastSeenAsync(serialNumber, cancellationToken);

            return "OK";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ADMS cdata upload failed for SN={SerialNumber}, table={Table}", serialNumber, table);
            return "OK";
        }
    }

    public async Task<string> HandleGetRequestAsync(string serialNumber, CancellationToken cancellationToken = default)
    {
        await TouchLastSeenAsync(serialNumber, cancellationToken);
        return "OK";
    }

    public async Task<string> HandleDeviceCmdAsync(string serialNumber, string body, CancellationToken cancellationToken = default)
    {
        await TouchLastSeenAsync(serialNumber, cancellationToken);
        return "OK";
    }

    public async Task<string> HandleRegistryAsync(string serialNumber, CancellationToken cancellationToken = default)
    {
        await TouchLastSeenAsync(serialNumber, cancellationToken);
        return "OK";
    }

    private async Task ProcessAttLogAsync(string serialNumber, string body, CancellationToken cancellationToken)
    {
        var device = await uow.BiometricDevices.GetBySerialNumberAsync(serialNumber, cancellationToken);
        var graceBefore = device?.ExamGraceMinutesBefore ?? 30;
        var graceAfter = device?.ExamGraceMinutesAfter ?? 30;

        var lines = (body ?? string.Empty)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.TrimEnd('\r'))
            .Where(l => !string.IsNullOrWhiteSpace(l));

        var processed = 0;
        foreach (var line in lines)
        {
            var parts = line.Split('\t');
            if (parts.Length < 2)
                continue;

            var pin = parts[0].Trim();
            if (string.IsNullOrWhiteSpace(pin))
                continue;

            if (!DateTime.TryParse(
                    parts[1].Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var punchTime))
                continue;

            punchTime = DateTime.SpecifyKind(punchTime, DateTimeKind.Utc);

            try
            {
                await processor.ProcessPunchAsync(
                    device?.Id, serialNumber, graceBefore, graceAfter, pin, punchTime, line, cancellationToken);
                processed++;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process ATTLOG line '{Line}' from SN={SerialNumber}", line, serialNumber);
            }
        }

        logger.LogInformation("Processed {Count} ATTLOG punch(es) from SN={SerialNumber}", processed, serialNumber);
    }

    private async Task<BiometricDeviceRegistry?> ResolveTenantAsync(string serialNumber, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(serialNumber))
            return null;

        var registry = await uow.BiometricDeviceRegistries.GetBySerialNumberAsync(serialNumber, cancellationToken);
        if (registry is null || !registry.IsActive)
            return null;

        var slug = registry.Tenant?.Slug ?? registry.SchemaName;
        var name = registry.Tenant?.Name ?? registry.SchemaName;
        tenantContext.SetTenant(registry.TenantId, slug, registry.SchemaName, name);
        return registry;
    }

    private async Task TouchLastSeenAsync(string serialNumber, CancellationToken cancellationToken)
    {
        try
        {
            var registry = await ResolveTenantAsync(serialNumber, cancellationToken);
            if (registry is null)
            {
                logger.LogWarning("ADMS request from unregistered device SN={SerialNumber}", serialNumber);
                return;
            }

            registry.LastSeenAt = DateTime.UtcNow;
            await uow.BiometricDeviceRegistries.UpdateAsync(registry, cancellationToken);
            await uow.SaveMasterChangesAsync(cancellationToken);
            await TouchTenantDeviceLastSeenAsync(serialNumber, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update last-seen for SN={SerialNumber}", serialNumber);
        }
    }

    private async Task TouchTenantDeviceLastSeenAsync(string serialNumber, CancellationToken cancellationToken)
    {
        var device = await uow.BiometricDevices.GetBySerialNumberAsync(serialNumber, cancellationToken);
        if (device is null)
            return;

        device.LastSeenAt = DateTime.UtcNow;
        await uow.BiometricDevices.UpdateAsync(device, cancellationToken);
        await uow.SaveTenantChangesAsync(cancellationToken);
    }
}
