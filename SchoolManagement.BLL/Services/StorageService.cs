using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.BLL.Settings;
using SchoolManagement.Common.Constants;

namespace SchoolManagement.BLL.Services;

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

    public async Task VerifyConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var buckets = await _minioClient.ListBucketsAsync(cancellationToken);
            _logger.LogInformation("MinIO connection verified. Buckets: {Count}", buckets.Buckets.Count);

            const string adminBucket = "school-platform-admin";
            var exists = await _minioClient.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(adminBucket), cancellationToken);

            if (!exists)
            {
                await _minioClient.MakeBucketAsync(
                    new MakeBucketArgs().WithBucket(adminBucket), cancellationToken);
                _logger.LogInformation("Created default admin bucket {Bucket}", adminBucket);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "MinIO connection verification failed. Storage features may be unavailable.");
        }
    }

    public async Task EnsureBucketAsync(string tenantSlug, CancellationToken cancellationToken = default)
    {
        var bucket = GetBucketName(tenantSlug);
        var exists = await _minioClient.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(bucket), cancellationToken);

        if (!exists)
        {
            await _minioClient.MakeBucketAsync(
                new MakeBucketArgs().WithBucket(bucket), cancellationToken);

            // Private by default — no anonymous policy (presigned URLs only)
            // Create folder placeholders
            foreach (var folder in new[]
                     {
                         AppConstants.StorageFolders.Logo,
                         AppConstants.StorageFolders.Avatars,
                         AppConstants.StorageFolders.Documents,
                         AppConstants.StorageFolders.Assignments,
                         AppConstants.StorageFolders.Reports
                     })
            {
                await PutPlaceholderAsync(bucket, $"{folder}/.keep", cancellationToken);
            }

            _logger.LogInformation("Created MinIO bucket {Bucket} for tenant {Slug}", bucket, tenantSlug);
        }
    }

    public async Task<string> UploadFileAsync(
        string tenantSlug,
        string folder,
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var bucket = GetBucketName(tenantSlug);
        await EnsureBucketAsync(tenantSlug, cancellationToken);

        var safeName = Path.GetFileName(fileName);
        var objectKey = $"{folder.Trim('/')}/{Guid.NewGuid():N}-{safeName}";

        await _minioClient.PutObjectAsync(new PutObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectKey)
            .WithStreamData(fileStream)
            .WithObjectSize(fileStream.Length)
            .WithContentType(contentType), cancellationToken);

        _logger.LogInformation("Uploaded {ObjectKey} to bucket {Bucket}", objectKey, bucket);
        return $"{bucket}/{objectKey}";
    }

    public async Task<string> UploadObjectAsync(
        string tenantSlug,
        string objectKey,
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var bucket = GetBucketName(tenantSlug);
        await EnsureBucketAsync(tenantSlug, cancellationToken);

        var key = objectKey.Trim().TrimStart('/');
        await _minioClient.PutObjectAsync(new PutObjectArgs()
            .WithBucket(bucket)
            .WithObject(key)
            .WithStreamData(fileStream)
            .WithObjectSize(fileStream.Length)
            .WithContentType(contentType), cancellationToken);

        _logger.LogInformation("Uploaded {ObjectKey} to bucket {Bucket}", key, bucket);
        return $"{bucket}/{key}";
    }

    public async Task<string> GetPresignedUrlAsync(
        string tenantSlug,
        string objectKey,
        CancellationToken cancellationToken = default)
    {
        var bucket = GetBucketName(tenantSlug);
        var key = objectKey.StartsWith($"{bucket}/")
            ? objectKey[(bucket.Length + 1)..]
            : objectKey;

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
        var bucket = GetBucketName(tenantSlug);
        var key = objectKey.StartsWith($"{bucket}/")
            ? objectKey[(bucket.Length + 1)..]
            : objectKey;

        await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
            .WithBucket(bucket)
            .WithObject(key), cancellationToken);

        _logger.LogInformation("Deleted {ObjectKey} from bucket {Bucket}", key, bucket);
    }

    public async Task DeleteBucketAsync(string tenantSlug, CancellationToken cancellationToken = default)
    {
        var bucket = GetBucketName(tenantSlug);
        var exists = await _minioClient.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(bucket), cancellationToken);
        if (!exists)
            return;

        await foreach (var item in _minioClient.ListObjectsEnumAsync(
                           new ListObjectsArgs().WithBucket(bucket).WithRecursive(true),
                           cancellationToken))
        {
            await _minioClient.RemoveObjectAsync(
                new RemoveObjectArgs().WithBucket(bucket).WithObject(item.Key),
                cancellationToken);
        }

        await _minioClient.RemoveBucketAsync(
            new RemoveBucketArgs().WithBucket(bucket), cancellationToken);

        _logger.LogInformation("Deleted MinIO bucket {Bucket}", bucket);
    }

    public async Task<long> GetBucketSizeBytesAsync(string tenantSlug, CancellationToken cancellationToken = default)
    {
        var bucket = GetBucketName(tenantSlug);
        var exists = await _minioClient.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(bucket), cancellationToken);
        if (!exists)
            return 0;

        long total = 0;
        await foreach (var item in _minioClient.ListObjectsEnumAsync(
                           new ListObjectsArgs().WithBucket(bucket).WithRecursive(true),
                           cancellationToken))
        {
            total += Convert.ToInt64(item.Size);
        }

        return total;
    }

    private static string GetBucketName(string tenantSlug)
    {
        var slug = tenantSlug.ToLowerInvariant().Trim();
        if (!System.Text.RegularExpressions.Regex.IsMatch(slug, @"^[a-z0-9]+(?:-[a-z0-9]+)*$"))
            throw new ArgumentException($"Invalid tenant slug for bucket: {tenantSlug}");

        return $"{AppConstants.BucketPrefix}{slug}";
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
