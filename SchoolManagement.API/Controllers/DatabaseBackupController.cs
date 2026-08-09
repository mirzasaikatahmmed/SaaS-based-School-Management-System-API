using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.API.Filters;
using SchoolManagement.BLL.DTOs.Settings;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/settings/backup")]
[Authorize]
public class DatabaseBackupController(IDatabaseBackupService service) : ControllerBase
{
    [HttpGet]
    [AuthorizePermission("Settings.DatabaseBackup", AppConstants.PermissionActions.View)]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        => Ok(ApiResponse<DatabaseBackupListDto>.Ok(await service.GetPagedAsync(page, pageSize, ct), "Backups retrieved"));

    [HttpPost("create")]
    [AuthorizePermission("Settings.DatabaseBackup", AppConstants.PermissionActions.Add)]
    public async Task<IActionResult> Create(CancellationToken ct = default)
        => Ok(ApiResponse<DatabaseBackupResponseDto>.Ok(await service.CreateAsync(ct), "Backup created"));

    [HttpGet("{id:guid}/download")]
    [AuthorizePermission("Settings.DatabaseBackup", AppConstants.PermissionActions.View)]
    public async Task<IActionResult> Download(Guid id, CancellationToken ct = default)
        => Ok(ApiResponse<DatabaseBackupDownloadDto>.Ok(await service.GetDownloadAsync(id, ct), "Download URL generated"));

    [HttpDelete("{id:guid}")]
    [AuthorizePermission("Settings.DatabaseBackup", AppConstants.PermissionActions.Delete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await service.DeleteAsync(id, ct);
        return Ok(ApiResponse.Ok("Backup deleted"));
    }

    [HttpPost("restore")]
    [AuthorizePermission("Settings.DatabaseBackup", AppConstants.PermissionActions.Edit)]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> Restore(IFormFile file, CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            throw new AppException("Backup file is required.", 400);
        await using var stream = file.OpenReadStream();
        await service.RestoreAsync(stream, file.FileName, ct);
        return Ok(ApiResponse.Ok("Backup restored"));
    }
}
