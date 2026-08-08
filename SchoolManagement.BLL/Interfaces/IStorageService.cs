namespace SchoolManagement.BLL.Interfaces;

public interface IStorageService
{
    Task EnsureBucketAsync(string tenantSlug, CancellationToken cancellationToken = default);
    Task<string> UploadFileAsync(string tenantSlug, string folder, Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task<string> UploadObjectAsync(string tenantSlug, string objectKey, Stream fileStream, string contentType, CancellationToken cancellationToken = default);
    Task<string> GetPresignedUrlAsync(string tenantSlug, string objectKey, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string tenantSlug, string objectKey, CancellationToken cancellationToken = default);
    Task DeleteBucketAsync(string tenantSlug, CancellationToken cancellationToken = default);
    Task<long> GetBucketSizeBytesAsync(string tenantSlug, CancellationToken cancellationToken = default);
    Task VerifyConnectionAsync(CancellationToken cancellationToken = default);
}
