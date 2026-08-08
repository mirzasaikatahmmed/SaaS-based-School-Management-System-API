using SchoolManagement.BLL.DTOs.Import;

namespace SchoolManagement.BLL.Interfaces;

public interface IStudentImportService
{
    byte[] GetSampleCsv();
    Task<ImportResultDto> ProcessImportAsync(
        Guid classId,
        Guid sectionId,
        Stream csvStream,
        string fileName,
        long fileLength,
        CancellationToken cancellationToken = default);
    Task<ImportBatchListResponseDto> GetBatchesAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<ImportBatchResponseDto> GetBatchAsync(Guid batchId, CancellationToken cancellationToken = default);
    Task<(byte[] Content, string FileName)> GetFailedRowsCsvAsync(Guid batchId, CancellationToken cancellationToken = default);
}
