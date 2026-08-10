using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.BLL.Settings;
using SchoolManagement.Common.Constants;

namespace SchoolManagement.BLL.Services;

/// <summary>
/// Shared MinIO bucket with school-based folders: {bucket}/{tenantSlug}/…
/// Legacy per-school buckets (school-{slug}) are still readable for old object keys.
/// </summary>
public class StorageService : IStorageService
{
    private readonly IMinioClient _minioClient;
    private readonly MinioSettings _settings;
    private readonly ILogger<StorageService> _logger;

    public StorageService(IOptions<MinioSettings> settings, ILogger<StorageService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        _minioClient = new MinioClient()
            .WithEndpoint(_settings.Endpoint)
            .WithCredentials(_settings.AccessKey, _settings.SecretKey)
            .WithSSL(_settings.UseSSL)
            .Build();
    }

    private string SharedBucket =>
        string.IsNullOrWhiteSpace(_settings.BucketName)
            ? AppConstants.DefaultSharedBucket
            : _settings.BucketName.Trim().ToLowerInvariant();

    public async Task VerifyConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var buckets = await _minioClient.ListBucketsAsync(cancellationToken);
            _logger.LogInformation("MinIO connection verified. Buckets: {Count}", buckets.Buckets.Count);
            await EnsureSharedBucketAsync(cancellationToken);

            // Platform folder placeholders (not per-school)
            foreach (var folder in new[] { "platform", "platform/logos" })
            {
                await PutPlaceholderAsync(SharedBucket, $"{folder}/.keep", cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "MinIO connection verification failed. Storage features may be unavailable.");
        }
    }

    public async Task EnsureBucketAsync(string tenantSlug, CancellationToken cancellationToken = default)
    {
        var slug = NormalizeSlug(tenantSlug);
        await EnsureSharedBucketAsync(cancellationToken);

        foreach (var folder in new[]
                 {
                     AppConstants.StorageFolders.Logo,
                     AppConstants.StorageFolders.Avatars,
                     AppConstants.StorageFolders.Documents,
                     AppConstants.StorageFolders.Assignments,
                     AppConstants.StorageFolders.Reports
                 })
        {
            await PutPlaceholderAsync(SharedBucket, $"{slug}/{folder}/.keep", cancellationToken);
        }

        _logger.LogInformation(
            "Ensured school folder {Prefix}/ in shared bucket {Bucket}",
            slug, SharedBucket);
    }

    public async Task<string> UploadFileAsync(
        string tenantSlug,
        string folder,
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var slug = NormalizeSlug(tenantSlug);
        await EnsureBucketAsync(slug, cancellationToken);

        var safeName = Path.GetFileName(fileName);
        var relativeKey = $"{folder.Trim('/')}/{Guid.NewGuid():N}-{safeName}";
        var objectKey = TenantObjectKey(slug, relativeKey);

        await _minioClient.PutObjectAsync(new PutObjectArgs()
            .WithBucket(SharedBucket)
            .WithObject(objectKey)
            .WithStreamData(fileStream)
            .WithObjectSize(fileStream.Length)
            .WithContentType(contentType), cancellationToken);

        _logger.LogInformation("Uploaded {ObjectKey} to bucket {Bucket}", objectKey, SharedBucket);
        return StorageLocator(objectKey);
    }

    public async Task<string> UploadObjectAsync(
        string tenantSlug,
        string objectKey,
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var slug = NormalizeSlug(tenantSlug);
        await EnsureBucketAsync(slug, cancellationToken);

        var key = TenantObjectKey(slug, objectKey);
        await _minioClient.PutObjectAsync(new PutObjectArgs()
            .WithBucket(SharedBucket)
            .WithObject(key)
            .WithStreamData(fileStream)
            .WithObjectSize(fileStream.Length)
            .WithContentType(contentType), cancellationToken);

        _logger.LogInformation("Uploaded {ObjectKey} to bucket {Bucket}", key, SharedBucket);
        return StorageLocator(key);
    }

    public async Task<string> GetPresignedUrlAsync(
        string tenantSlug,
        string objectKey,
        CancellationToken cancellationToken = default)
    {
        var (bucket, key) = ResolveObjectLocation(tenantSlug, objectKey);
        return await _minioClient.PresignedGetObjectAsync(new PresignedGetObjectArgs()
            .WithBucket(bucket)
            .WithObject(key)
            .WithExpiry(AppConstants.PresignedUrlExpirySeconds));
    }

    public async Task DeleteFileAsync(
        string tenantSlug,
        string objectKey,
        CancellationToken cancellationToken = default)
    {
        var (bucket, key) = ResolveObjectLocation(tenantSlug, objectKey);
        await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
            .WithBucket(bucket)
            .WithObject(key), cancellationToken);

        _logger.LogInformation("Deleted {ObjectKey} from bucket {Bucket}", key, bucket);
    }

    public async Task DeleteBucketAsync(string tenantSlug, CancellationToken cancellationToken = default)
    {
        var slug = NormalizeSlug(tenantSlug);
        var prefix = $"{slug}/";

        // Remove school folder from shared bucket
        await EnsureSharedBucketAsync(cancellationToken);
        await foreach (var item in _minioClient.ListObjectsEnumAsync(
                           new ListObjectsArgs()
                               .WithBucket(SharedBucket)
                               .WithPrefix(prefix)
                               .WithRecursive(true),
                           cancellationToken))
        {
            await _minioClient.RemoveObjectAsync(
                new RemoveObjectArgs().WithBucket(SharedBucket).WithObject(item.Key),
                cancellationToken);
        }

        _logger.LogInformation(
            "Deleted school folder {Prefix} from shared bucket {Bucket}",
            prefix, SharedBucket);

        // Also clean up legacy per-school bucket if it still exists
        var legacyBucket = $"{AppConstants.LegacyBucketPrefix}{slug}";
        var legacyExists = await _minioClient.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(legacyBucket), cancellationToken);
        if (!legacyExists)
            return;

        await foreach (var item in _minioClient.ListObjectsEnumAsync(
                           new ListObjectsArgs().WithBucket(legacyBucket).WithRecursive(true),
                           cancellationToken))
        {
            await _minioClient.RemoveObjectAsync(
                new RemoveObjectArgs().WithBucket(legacyBucket).WithObject(item.Key),
                cancellationToken);
        }

        await _minioClient.RemoveBucketAsync(
            new RemoveBucketArgs().WithBucket(legacyBucket), cancellationToken);
        _logger.LogInformation("Deleted legacy MinIO bucket {Bucket}", legacyBucket);
    }

    public async Task<long> GetBucketSizeBytesAsync(string tenantSlug, CancellationToken cancellationToken = default)
    {
        var slug = NormalizeSlug(tenantSlug);
        long total = 0;

        var sharedExists = await _minioClient.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(SharedBucket), cancellationToken);
        if (sharedExists)
        {
            await foreach (var item in _minioClient.ListObjectsEnumAsync(
                               new ListObjectsArgs()
                                   .WithBucket(SharedBucket)
                                   .WithPrefix($"{slug}/")
                                   .WithRecursive(true),
                               cancellationToken))
            {
                total += Convert.ToInt64(item.Size);
            }
        }

        var legacyBucket = $"{AppConstants.LegacyBucketPrefix}{slug}";
        var legacyExists = await _minioClient.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(legacyBucket), cancellationToken);
        if (legacyExists)
        {
            await foreach (var item in _minioClient.ListObjectsEnumAsync(
                               new ListObjectsArgs().WithBucket(legacyBucket).WithRecursive(true),
                               cancellationToken))
            {
                total += Convert.ToInt64(item.Size);
            }
        }

        return total;
    }

    private async Task EnsureSharedBucketAsync(CancellationToken cancellationToken)
    {
        var bucket = SharedBucket;
        var exists = await _minioClient.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(bucket), cancellationToken);
        if (exists)
            return;

        await _minioClient.MakeBucketAsync(
            new MakeBucketArgs().WithBucket(bucket), cancellationToken);
        _logger.LogInformation("Created shared MinIO bucket {Bucket}", bucket);
    }

    /// <summary>
    /// Resolves (bucket, objectKey) for reads/deletes.
    /// Supports shared-bucket locators, relative keys, and legacy school-{slug} buckets.
    /// </summary>
    private (string Bucket, string Key) ResolveObjectLocation(string tenantSlug, string objectKey)
    {
        var slug = NormalizeSlug(tenantSlug);
        var raw = objectKey.Trim().TrimStart('/');
        if (string.IsNullOrWhiteSpace(raw))
            throw new ArgumentException("Object key is required.", nameof(objectKey));

        var shared = SharedBucket;
        var legacyBucket = $"{AppConstants.LegacyBucketPrefix}{slug}";

        // school-mgmt/riverside/students/...
        if (raw.StartsWith($"{shared}/", StringComparison.OrdinalIgnoreCase))
        {
            var key = raw[(shared.Length + 1)..];
            return (shared, EnsureTenantPrefix(slug, key));
        }

        // Legacy stored locator: school-riverside/students/...
        if (raw.StartsWith($"{legacyBucket}/", StringComparison.OrdinalIgnoreCase))
        {
            return (legacyBucket, raw[(legacyBucket.Length + 1)..]);
        }

        // Already tenant-prefixed: riverside/students/...
        if (raw.StartsWith($"{slug}/", StringComparison.OrdinalIgnoreCase))
            return (shared, raw);

        // Relative: students/... → riverside/students/...
        return (shared, $"{slug}/{raw}");
    }

    private static string TenantObjectKey(string slug, string objectKey)
    {
        var key = objectKey.Trim().TrimStart('/');
        // Avoid double-prefix if caller already passed slug/...
        if (key.StartsWith($"{slug}/", StringComparison.OrdinalIgnoreCase))
            return key;
        // Strip accidental shared-bucket prefix from callers
        return $"{slug}/{key}";
    }

    private static string EnsureTenantPrefix(string slug, string key)
    {
        var k = key.Trim().TrimStart('/');
        if (k.StartsWith($"{slug}/", StringComparison.OrdinalIgnoreCase))
            return k;
        return $"{slug}/{k}";
    }

    private string StorageLocator(string objectKeyInsideSharedBucket)
        => $"{SharedBucket}/{objectKeyInsideSharedBucket.TrimStart('/')}";

    private static string NormalizeSlug(string tenantSlug)
    {
        var slug = tenantSlug.ToLowerInvariant().Trim();
        if (!System.Text.RegularExpressions.Regex.IsMatch(slug, @"^[a-z0-9]+(?:-[a-z0-9]+)*$"))
            throw new ArgumentException($"Invalid tenant slug for storage: {tenantSlug}");
        return slug;
    }

    private async Task PutPlaceholderAsync(string bucket, string objectKey, CancellationToken cancellationToken)
    {
        var empty = new MemoryStream(Array.Empty<byte>());
        await _minioClient.PutObjectAsync(new PutObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectKey)
            .WithStreamData(empty)
            .WithObjectSize(0)
            .WithContentType("application/octet-stream"), cancellationToken);
    }
}
