using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.Student;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/admission/lookup")]
[Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Teacher},{AppConstants.Roles.Receptionist}")]
public class AdmissionLookupController : ControllerBase
{
    private readonly IStudentService _studentService;

    public AdmissionLookupController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    [HttpGet("academic-years")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<int>>>> GetAcademicYears(
        CancellationToken cancellationToken = default)
    {
        var result = await _studentService.GetAcademicYearsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<int>>.Ok(result, "Academic years retrieved"));
    }

    [HttpGet("classes")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AdmissionLookupItemDto>>>> GetClasses(
        CancellationToken cancellationToken = default)
    {
        var result = await _studentService.GetClassesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AdmissionLookupItemDto>>.Ok(result, "Classes retrieved"));
    }

    [HttpGet("sections/{classId:guid}")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AdmissionLookupItemDto>>>> GetSections(
        Guid classId,
        CancellationToken cancellationToken = default)
    {
        var result = await _studentService.GetSectionsAsync(classId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AdmissionLookupItemDto>>.Ok(result, "Sections retrieved"));
    }

    [HttpGet("categories")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AdmissionLookupItemDto>>>> GetCategories(
        CancellationToken cancellationToken = default)
    {
        var result = await _studentService.GetCategoriesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AdmissionLookupItemDto>>.Ok(result, "Categories retrieved"));
    }

    [HttpGet("transport-routes")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AdmissionLookupItemDto>>>> GetTransportRoutes(
        CancellationToken cancellationToken = default)
    {
        var result = await _studentService.GetTransportRoutesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AdmissionLookupItemDto>>.Ok(result, "Transport routes retrieved"));
    }

    [HttpGet("hostels")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AdmissionLookupItemDto>>>> GetHostels(
        CancellationToken cancellationToken = default)
    {
        var result = await _studentService.GetHostelsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AdmissionLookupItemDto>>.Ok(result, "Hostels retrieved"));
    }

    [HttpGet("hostel-rooms/{hostelId:guid}")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AdmissionLookupItemDto>>>> GetHostelRooms(
        Guid hostelId,
        CancellationToken cancellationToken = default)
    {
        var result = await _studentService.GetHostelRoomsAsync(hostelId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AdmissionLookupItemDto>>.Ok(result, "Hostel rooms retrieved"));
    }

    [HttpGet("next-register-no")]
    public async Task<ActionResult<ApiResponse<NextRegisterNoDto>>> GetNextRegisterNo(
        [FromQuery] int? academicYear,
        CancellationToken cancellationToken = default)
    {
        var result = await _studentService.GetNextRegisterNoAsync(academicYear, cancellationToken);
        return Ok(ApiResponse<NextRegisterNoDto>.Ok(result, "Next register number generated"));
    }
}
