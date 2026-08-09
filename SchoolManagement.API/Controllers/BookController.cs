using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.Library;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/library/books")]
[Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Librarian}")]
public class BookController(IBookService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] BookFilterDto filter, CancellationToken ct = default)
        => Ok(ApiResponse<BookListResponseDto>.Ok(await service.GetListAsync(filter, ct), "Books retrieved"));

    [HttpGet("lookup")]
    public async Task<IActionResult> Lookup(CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<BookLookupDto>>.Ok(await service.GetLookupAsync(ct), "Books lookup retrieved"));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct = default)
        => Ok(ApiResponse<BookDetailDto>.Ok(await service.GetByIdAsync(id, ct), "Book retrieved"));

    [HttpPost]
    public async Task<IActionResult> Create(CreateBookDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<BookDetailDto>.Ok(await service.CreateAsync(dto, ct), "Book created"));

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateBookDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<BookDetailDto>.Ok(await service.UpdateAsync(id, dto, ct), "Book updated"));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await service.DeleteAsync(id, ct);
        return Ok(ApiResponse.Ok("Book deleted"));
    }

    [HttpPost("{id:guid}/cover")]
    [RequestSizeLimit(3 * 1024 * 1024)]
    public async Task<IActionResult> UploadCover(Guid id, IFormFile file, CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse.Fail("File is required."));
        await using var stream = file.OpenReadStream();
        return Ok(ApiResponse<BookDetailDto>.Ok(await service.UploadCoverAsync(id, stream, file.FileName, file.ContentType, ct), "Book cover uploaded"));
    }
}
