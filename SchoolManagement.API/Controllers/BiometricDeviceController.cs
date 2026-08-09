using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.Biometric;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/biometric/devices")]
[Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Accountant}")]
public class BiometricDeviceController(IBiometricDeviceService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<DeviceResponseDto>>.Ok(await service.GetAllAsync(ct), "Devices retrieved"));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
        => Ok(ApiResponse<DeviceResponseDto>.Ok(await service.GetByIdAsync(id, ct), "Device retrieved"));

    [HttpPost]
    public async Task<IActionResult> Create(CreateDeviceDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<DeviceResponseDto>.Ok(await service.CreateAsync(dto, ct), "Device registered"));

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateDeviceDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<DeviceResponseDto>.Ok(await service.UpdateAsync(id, dto, ct), "Device updated"));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await service.DeleteAsync(id, ct);
        return Ok(ApiResponse.Ok("Device removed"));
    }
}
