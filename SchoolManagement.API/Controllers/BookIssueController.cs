using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.Library;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/library/issues")]
[Authorize]
public class BookIssueController(IBookIssueService service) : ControllerBase
{
    private const string ManageRoles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Librarian}";

    [HttpGet]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> GetList([FromQuery] BookIssueFilterDto filter, CancellationToken ct = default)
        => Ok(ApiResponse<BookIssueListResponseDto>.Ok(await service.GetListAsync(filter, ct), "Book issues retrieved"));

    [HttpGet("my")]
    public async Task<IActionResult> GetMy(CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<BookIssueListItemDto>>.Ok(await service.GetMyAsync(ct), "My book issues retrieved"));

    [HttpGet("lookup/borrowers")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Borrowers([FromQuery] string role, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<BorrowerLookupDto>>.Ok(await service.GetBorrowersLookupAsync(role, ct), "Borrowers retrieved"));

    [HttpPost]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Issue(IssueBookDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<BookIssueListItemDto>.Ok(await service.IssueAsync(dto, ct), "Book issued"));

    [HttpPatch("{id:guid}/return")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Return(Guid id, ReturnBookDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<BookIssueListItemDto>.Ok(await service.ReturnAsync(id, dto, ct), "Book returned"));
}
