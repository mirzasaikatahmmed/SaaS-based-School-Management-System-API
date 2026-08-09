using System.Diagnostics;
using System.IO.Compression;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SchoolManagement.BLL.DTOs.Settings;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class DatabaseBackupService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    ITenantDbContextFactory tenantDbFactory,
    IStorageService storage,
    IConfiguration configuration,
    IHttpContextAccessor http,
    ILogger<DatabaseBackupService> logger) : IDatabaseBackupService
{
    public async Task<DatabaseBackupListDto> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        Manage();
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 200 ? 20 : pageSize;
        var (items, total) = await uow.DatabaseBackups.GetPagedAsync(page, pageSize, cancellationToken);
        return new DatabaseBackupListDto
        {
            Data = items.Select(Map).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize)
        };
    }

    public async Task<DatabaseBackupResponseDto> CreateAsync(CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        Manage();
        var (stream, fileName, note) = await CreateDumpAsync(cancellationToken);
        await using (stream)
        {
            var size = stream.Length;
            stream.Position = 0;
            await storage.EnsureBucketAsync(tenant.TenantSlug!, cancellationToken);
            var objectKey = await storage.UploadFileAsync(
                tenant.TenantSlug!,
                AppConstants.StorageFolders.DbBackups,
                stream,
                fileName,
                "application/zip",
                cancellationToken);

            var backup = new DatabaseBackup
            {
                Id = Guid.NewGuid(),
                FileName = fileName,
                ObjectKey = objectKey,
                SizeBytes = size,
                Note = note,
                CreatedBy = CurrentUserId(),
                CreatedAt = DateTime.UtcNow
            };
            await uow.DatabaseBackups.AddAsync(backup, cancellationToken);
            await uow.SaveTenantChangesAsync(cancellationToken);
            return Map(backup);
        }
    }

    public async Task<DatabaseBackupDownloadDto> GetDownloadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        Manage();
        var backup = await uow.DatabaseBackups.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Backup '{id}' not found.");
        var url = await storage.GetPresignedUrlAsync(tenant.TenantSlug!, backup.ObjectKey, cancellationToken);
        return new DatabaseBackupDownloadDto
        {
            Id = backup.Id,
            FileName = backup.FileName,
            DownloadUrl = url
        };
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        Manage();
        var backup = await uow.DatabaseBackups.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Backup '{id}' not found.");
        try
        {
            await storage.DeleteFileAsync(tenant.TenantSlug!, backup.ObjectKey, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to delete backup object {Key} from MinIO", backup.ObjectKey);
        }

        await uow.DatabaseBackups.DeleteAsync(backup, cancellationToken);
        await uow.SaveTenantChangesAsync(cancellationToken);
    }

    public async Task RestoreAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        Manage();

        // Safety backup first
        await CreateAsync(cancellationToken);

        await using var ms = new MemoryStream();
        await fileStream.CopyToAsync(ms, cancellationToken);
        ms.Position = 0;

        string sql;
        if (fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            using var archive = new ZipArchive(ms, ZipArchiveMode.Read, leaveOpen: true);
            var entry = archive.Entries.FirstOrDefault(e => e.Name.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
                ?? throw new AppException("Zip archive must contain a .sql file.", 400);
            await using var entryStream = entry.Open();
            using var reader = new StreamReader(entryStream, Encoding.UTF8);
            sql = await reader.ReadToEndAsync(cancellationToken);
        }
        else if (fileName.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
        {
            using var reader = new StreamReader(ms, Encoding.UTF8, leaveOpen: true);
            sql = await reader.ReadToEndAsync(cancellationToken);
        }
        else
            throw new AppException("Only .sql or .zip backups can be restored.", 400);

        if (string.IsNullOrWhiteSpace(sql))
            throw new AppException("Backup file is empty.", 400);

        await using var db = tenantDbFactory.Create(tenant.SchemaName!);
        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await db.Database.ExecuteSqlRawAsync(sql, cancellationToken);
            await tx.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(cancellationToken);
            throw new AppException($"Restore failed and was rolled back: {ex.Message}", 500);
        }
    }

    private async Task<(MemoryStream Stream, string FileName, string? Note)> CreateDumpAsync(CancellationToken ct)
    {
        var schema = tenant.SchemaName!;
        var stamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var sqlName = $"{schema}_{stamp}.sql";
        var zipName = $"{schema}_{stamp}.zip";
        var conn = configuration.GetConnectionString("MasterDb")
            ?? throw new AppException("MasterDb connection string is missing.", 500);

        string? note = null;
        byte[] sqlBytes;
        try
        {
            sqlBytes = await RunPgDumpAsync(conn, schema, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "pg_dump failed; writing placeholder schema export");
            note = $"pg_dump unavailable ({ex.Message}). Placeholder export created.";
            sqlBytes = Encoding.UTF8.GetBytes(
                $"-- Placeholder backup for {schema} at {DateTime.UtcNow:O}\n-- Install pg_dump for full schema dumps.\n");
        }

        var zipStream = new MemoryStream();
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entry = archive.CreateEntry(sqlName, CompressionLevel.Optimal);
            await using var entryStream = entry.Open();
            await entryStream.WriteAsync(sqlBytes, ct);
        }

        zipStream.Position = 0;
        return (zipStream, zipName, note);
    }

    private static async Task<byte[]> RunPgDumpAsync(string connectionString, string schema, CancellationToken ct)
    {
        var host = GetConnValue(connectionString, "Host") ?? "localhost";
        var port = GetConnValue(connectionString, "Port") ?? "5432";
        var database = GetConnValue(connectionString, "Database") ?? "postgres";
        var username = GetConnValue(connectionString, "Username") ?? GetConnValue(connectionString, "User ID") ?? "postgres";
        var password = GetConnValue(connectionString, "Password") ?? string.Empty;

        var args = $"--dbname=postgresql://{Uri.EscapeDataString(username)}:{Uri.EscapeDataString(password)}@{host}:{port}/{database} -n {schema} --no-owner --no-acl";
        var psi = new ProcessStartInfo
        {
            FileName = "pg_dump",
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        using var process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start pg_dump.");
        await using var ms = new MemoryStream();
        await process.StandardOutput.BaseStream.CopyToAsync(ms, ct);
        var stderr = await process.StandardError.ReadToEndAsync(ct);
        await process.WaitForExitAsync(ct);
        if (process.ExitCode != 0)
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(stderr) ? $"pg_dump exit {process.ExitCode}" : stderr);
        return ms.ToArray();
    }

    private static string? GetConnValue(string connectionString, string key)
    {
        foreach (var part in connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var idx = part.IndexOf('=');
            if (idx <= 0) continue;
            if (part[..idx].Trim().Equals(key, StringComparison.OrdinalIgnoreCase))
                return part[(idx + 1)..].Trim();
        }
        return null;
    }

    private static DatabaseBackupResponseDto Map(DatabaseBackup b) => new()
    {
        Id = b.Id,
        FileName = b.FileName,
        SizeBytes = b.SizeBytes,
        Note = b.Note,
        CreatedAt = b.CreatedAt
    };

    private Guid? CurrentUserId()
    {
        var raw = http.HttpContext?.User.FindFirst(AppConstants.Claims.UserId)?.Value
            ?? http.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(raw, out var id) ? id : null;
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
            throw new ForbiddenException("Only Super Admin or School Admin can manage backups.");
    }
}
