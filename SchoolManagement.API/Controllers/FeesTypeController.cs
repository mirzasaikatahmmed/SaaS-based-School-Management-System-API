using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.StudentAccounting;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/student-accounting/fees-types")]
[Authorize]
public class FeesTypeController(IFeesTypeService service) : ControllerBase
{
    private const string ManageRoles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Accountant}";

    [HttpGet]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<FeesTypeResponseDto>>.Ok(await service.GetAllAsync(isActive, ct), "Fees types retrieved"));

    [HttpGet("lookup")]
    public async Task<IActionResult> Lookup(CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<FeesTypeLookupDto>>.Ok(await service.GetLookupAsync(ct), "Fees types retrieved"));

    [HttpGet("{id:guid}")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
        => Ok(ApiResponse<FeesTypeResponseDto>.Ok(await service.GetByIdAsync(id, ct), "Fees type retrieved"));

    [HttpPost]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Create(CreateFeesTypeDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<FeesTypeResponseDto>.Ok(await service.CreateAsync(dto, ct), "Fees type created"));

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Update(Guid id, UpdateFeesTypeDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<FeesTypeResponseDto>.Ok(await service.UpdateAsync(id, dto, ct), "Fees type updated"));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await service.DeleteAsync(id, ct);
        return Ok(ApiResponse.Ok("Fees type deleted"));
    }
}
