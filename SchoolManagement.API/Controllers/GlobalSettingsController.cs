using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.Settings;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/settings/global")]
[Authorize(Roles = AppConstants.Roles.SuperAdmin)]
public class GlobalSettingsController(IGlobalSettingsService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct = default)
        => Ok(ApiResponse<GlobalSettingsResponseDto>.Ok(await service.GetAsync(ct), "Global settings retrieved"));

    [HttpPatch]
    public async Task<IActionResult> UpdateGeneral(UpdateGlobalGeneralDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<GlobalSettingsResponseDto>.Ok(await service.UpdateGeneralAsync(dto, ct), "Global settings updated"));

    [HttpPatch("upload-file")]
    public async Task<IActionResult> UpdateUploadFile(UpdateGlobalUploadFileDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<GlobalSettingsResponseDto>.Ok(await service.UpdateUploadFileAsync(dto, ct), "Upload file settings updated"));
}
