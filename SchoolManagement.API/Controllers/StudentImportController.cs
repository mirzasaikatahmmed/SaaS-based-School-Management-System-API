using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.Import;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/student-import")]
[Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
public class StudentImportController : ControllerBase
{
    private readonly IStudentImportService _importService;

    public StudentImportController(IStudentImportService importService)
    {
        _importService = importService;
    }

    [HttpGet("sample-csv")]
    public IActionResult DownloadSampleCsv()
    {
        var bytes = _importService.GetSampleCsv();
        return File(bytes, "text/csv", "student_import_sample.csv");
    }

    [HttpPost]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<ImportResultDto>>> Import(
        [FromForm] Guid classId,
        [FromForm] Guid sectionId,
        IFormFile csvFile,
        CancellationToken cancellationToken = default)
    {
        if (csvFile is null || csvFile.Length == 0)
            return BadRequest(ApiResponse.Fail("CsvFile is required."));

        await using var stream = csvFile.OpenReadStream();
        var result = await _importService.ProcessImportAsync(
            classId, sectionId, stream, csvFile.FileName, csvFile.Length, cancellationToken);

        return Ok(ApiResponse<ImportResultDto>.Ok(result, "Import completed"));
    }

    [HttpGet("batches")]
    public async Task<ActionResult<ApiResponse<ImportBatchListResponseDto>>> GetBatches(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _importService.GetBatchesAsync(page, pageSize, cancellationToken);
        return Ok(ApiResponse<ImportBatchListResponseDto>.Ok(result, "Import batches retrieved"));
    }

    [HttpGet("batches/{batchId:guid}")]
    public async Task<ActionResult<ApiResponse<ImportBatchResponseDto>>> GetBatch(
        Guid batchId,
        CancellationToken cancellationToken = default)
    {
        var result = await _importService.GetBatchAsync(batchId, cancellationToken);
        return Ok(ApiResponse<ImportBatchResponseDto>.Ok(result, "Import batch retrieved"));
    }

    [HttpGet("batches/{batchId:guid}/errors")]
    public async Task<IActionResult> DownloadFailedRows(
        Guid batchId,
        CancellationToken cancellationToken = default)
    {
        var (content, fileName) = await _importService.GetFailedRowsCsvAsync(batchId, cancellationToken);
        return File(content, "text/csv", fileName);
    }
}
