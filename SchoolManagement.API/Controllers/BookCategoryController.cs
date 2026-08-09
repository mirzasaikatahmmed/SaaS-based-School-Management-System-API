using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.Library;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/library/categories")]
[Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Librarian}")]
public class BookCategoryController(IBookCategoryService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<BookCategoryDto>>.Ok(await service.GetAllAsync(ct), "Book categories retrieved"));

    [HttpPost]
    public async Task<IActionResult> Create(CreateBookCategoryDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<BookCategoryDto>.Ok(await service.CreateAsync(dto, ct), "Book category created"));

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateBookCategoryDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<BookCategoryDto>.Ok(await service.UpdateAsync(id, dto, ct), "Book category updated"));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await service.DeleteAsync(id, ct);
        return Ok(ApiResponse.Ok("Book category deleted"));
    }
}
