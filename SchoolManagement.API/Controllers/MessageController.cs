using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.DTOs.Message;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/messages")]
[Authorize]
public class MessageController(IMessageService service) : ControllerBase
{
    [HttpGet("inbox")]
    public async Task<IActionResult> Inbox([FromQuery] MessageFilterDto filter, CancellationToken ct = default)
        => Ok(ApiResponse<MessageListResponseDto>.Ok(await service.GetInboxAsync(filter, ct), "Inbox retrieved"));

    [HttpGet("sent")]
    public async Task<IActionResult> Sent([FromQuery] MessageFilterDto filter, CancellationToken ct = default)
        => Ok(ApiResponse<MessageListResponseDto>.Ok(await service.GetSentAsync(filter, ct), "Sent messages retrieved"));

    [HttpGet("important")]
    public async Task<IActionResult> Important([FromQuery] MessageFilterDto filter, CancellationToken ct = default)
        => Ok(ApiResponse<MessageListResponseDto>.Ok(await service.GetImportantAsync(filter, ct), "Important messages retrieved"));

    [HttpGet("trash")]
    public async Task<IActionResult> Trash([FromQuery] MessageFilterDto filter, CancellationToken ct = default)
        => Ok(ApiResponse<MessageListResponseDto>.Ok(await service.GetTrashAsync(filter, ct), "Trash retrieved"));

    [HttpGet("unread-count")]
    public async Task<IActionResult> UnreadCount(CancellationToken ct = default)
        => Ok(ApiResponse<UnreadCountDto>.Ok(await service.GetUnreadCountAsync(ct), "Unread count retrieved"));

    [HttpGet("recipients/lookup")]
    public async Task<IActionResult> RecipientLookup(
        [FromQuery] string? role, [FromQuery] Guid? classId, [FromQuery] Guid? sectionId, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<RecipientLookupDto>>.Ok(
            await service.GetRecipientLookupAsync(role, classId, sectionId, ct), "Recipients retrieved"));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
        => Ok(ApiResponse<MessageDetailDto>.Ok(await service.GetByIdAsync(id, ct), "Message retrieved"));

    [HttpPost]
    public async Task<IActionResult> Compose(ComposeMessageDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<MessageDetailDto>.Ok(await service.ComposeAsync(dto, ct), "Message sent"));

    [HttpPost("{id:guid}/reply")]
    public async Task<IActionResult> Reply(Guid id, ReplyMessageDto dto, CancellationToken ct = default)
        => Ok(ApiResponse<MessageDetailDto>.Ok(await service.ReplyAsync(id, dto, ct), "Reply sent"));

    [HttpPost("{id:guid}/attachment")]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> UploadAttachment(Guid id, IFormFile file, CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            throw new AppException("Attachment file is required.", 400);

        await using var stream = file.OpenReadStream();
        var result = await service.UploadAttachmentAsync(id, stream, file.FileName, file.ContentType, ct);
        return Ok(ApiResponse<MessageDetailDto>.Ok(result, "Attachment uploaded"));
    }

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct = default)
        => Ok(ApiResponse<MessageDetailDto>.Ok(await service.MarkReadAsync(id, ct), "Message marked as read"));

    [HttpPatch("{id:guid}/important")]
    public async Task<IActionResult> ToggleImportant(Guid id, CancellationToken ct = default)
        => Ok(ApiResponse<MessageDetailDto>.Ok(await service.ToggleImportantAsync(id, ct), "Message importance toggled"));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await service.DeleteAsync(id, ct);
        return Ok(ApiResponse.Ok("Message moved to trash"));
    }

    [HttpDelete("{id:guid}/permanent")]
    public async Task<IActionResult> PermanentDelete(Guid id, CancellationToken ct = default)
    {
        await service.PermanentDeleteAsync(id, ct);
        return Ok(ApiResponse.Ok("Message permanently deleted"));
    }

    [HttpPost("{id:guid}/restore")]
    public async Task<IActionResult> Restore(Guid id, CancellationToken ct = default)
        => Ok(ApiResponse<MessageDetailDto>.Ok(await service.RestoreAsync(id, ct), "Message restored"));
}
