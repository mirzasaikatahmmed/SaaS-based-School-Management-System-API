using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.Academic;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/academic/student-electives")]
[Authorize]
public class StudentElectiveController(IStudentElectiveService service) : ControllerBase
{
    private const string ManageRoles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}";
    private const string ReadRoles = $"{ManageRoles},{AppConstants.Roles.Teacher}";

    [HttpGet]
    [Authorize(Roles = ReadRoles)]
    public async Task<IActionResult> List(
        [FromQuery] Guid classId,
        [FromQuery] Guid sectionId,
        [FromQuery] int academicYear,
        [FromQuery] string electiveGroup = "4th",
        CancellationToken ct = default)
        => Ok(ApiResponse<StudentElectiveListDto>.Ok(
            await service.GetClassElectivesAsync(classId, sectionId, academicYear, electiveGroup, ct),
            "Student electives retrieved"));

    [HttpPatch]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Assign(AssignStudentElectiveDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<StudentElectiveRowDto>.Ok(
            await service.AssignAsync(dto, ct),
            "4th subject assigned"));

    [HttpPatch("bulk")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Bulk(BulkAssignStudentElectiveDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<StudentElectiveListDto>.Ok(
            await service.BulkAssignAsync(dto, ct),
            "Electives assigned"));
}
