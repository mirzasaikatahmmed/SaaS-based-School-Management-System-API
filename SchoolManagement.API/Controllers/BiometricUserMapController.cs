using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.Biometric;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/biometric/maps")]
[Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Accountant}")]
public class BiometricUserMapController(IBiometricUserMapService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<UserMapResponseDto>>.Ok(await service.GetAllAsync(ct), "Mappings retrieved"));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
        => Ok(ApiResponse<UserMapResponseDto>.Ok(await service.GetByIdAsync(id, ct), "Mapping retrieved"));

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserMapDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<UserMapResponseDto>.Ok(await service.CreateAsync(dto, ct), "Mapping created"));

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateUserMapDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<UserMapResponseDto>.Ok(await service.UpdateAsync(id, dto, ct), "Mapping updated"));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await service.DeleteAsync(id, ct);
        return Ok(ApiResponse.Ok("Mapping removed"));
    }
}
