using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.StudentAccounting;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/student-accounting/invoices")]
[Authorize]
public class StudentFeeInvoiceController(IStudentFeeInvoiceService service) : ControllerBase
{
    private const string ManageRoles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Accountant}";

    [HttpGet]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> GetFiltered([FromQuery] StudentFeeInvoiceFilterDto filter, CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(filter.Export))
        {
            var file = await service.ExportAsync(filter, ct);
            return File(file.Content, file.ContentType, file.FileName);
        }
        return Ok(ApiResponse<StudentFeeInvoiceListResponseDto>.Ok(await service.GetFilteredAsync(filter, ct), "Invoices retrieved"));
    }

    [HttpGet("export")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Export([FromQuery] StudentFeeInvoiceFilterDto filter, CancellationToken ct = default)
    {
        var file = await service.ExportAsync(filter, ct);
        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("my")]
    [Authorize(Roles = $"{AppConstants.Roles.Student},{AppConstants.Roles.Parent}")]
    public async Task<IActionResult> GetMy(CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<StudentFeeInvoiceResponseDto>>.Ok(await service.GetMyInvoicesAsync(ct), "My invoices retrieved"));

    [HttpGet("student/{studentId:guid}")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> GetByStudent(Guid studentId, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<StudentFeeInvoiceResponseDto>>.Ok(await service.GetByStudentAsync(studentId, ct), "Invoices retrieved"));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
        => Ok(ApiResponse<StudentFeeInvoiceResponseDto>.Ok(await service.GetByIdAsync(id, ct), "Invoice retrieved"));

    [HttpPatch("{id:guid}/pay")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Pay(Guid id, PayInvoiceDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<StudentFeeInvoiceResponseDto>.Ok(await service.PayAsync(id, dto, ct), "Payment recorded"));

    [HttpPost("generate")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Generate(GenerateInvoicesDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<GenerateInvoicesResultDto>.Ok(await service.GenerateAsync(dto, ct), "Invoices generated"));
}
