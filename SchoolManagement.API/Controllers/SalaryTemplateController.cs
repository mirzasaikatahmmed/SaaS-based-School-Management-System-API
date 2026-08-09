using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.Payroll;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/payroll/salary-templates")]
[Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Accountant}")]
public class SalaryTemplateController(ISalaryTemplateService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList(CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<SalaryTemplateListItemDto>>.Ok(await service.GetListAsync(ct), "Salary templates retrieved"));

    [HttpGet("lookup")]
    public async Task<IActionResult> Lookup(CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<SalaryTemplateLookupDto>>.Ok(await service.GetLookupAsync(ct), "Salary template lookup retrieved"));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct = default)
        => Ok(ApiResponse<SalaryTemplateResponseDto>.Ok(await service.GetByIdAsync(id, ct), "Salary template retrieved"));

    [HttpPost]
    public async Task<IActionResult> Create(CreateSalaryTemplateDto dto, CancellationToken ct = default)
    {
        var result = await service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, ApiResponse<SalaryTemplateResponseDto>.Ok(result, "Salary template created"));
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateSalaryTemplateDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<SalaryTemplateResponseDto>.Ok(await service.UpdateAsync(id, dto, ct), "Salary template updated"));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await service.DeleteAsync(id, ct);
        return Ok(ApiResponse.Ok("Salary template deleted"));
    }
}
