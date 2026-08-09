using SchoolManagement.BLL.DTOs.Message;

namespace SchoolManagement.BLL.Interfaces;

public interface IMessageService
{
    Task<MessageListResponseDto> GetInboxAsync(MessageFilterDto filter, CancellationToken cancellationToken = default);
    Task<MessageListResponseDto> GetSentAsync(MessageFilterDto filter, CancellationToken cancellationToken = default);
    Task<MessageListResponseDto> GetImportantAsync(MessageFilterDto filter, CancellationToken cancellationToken = default);
    Task<MessageListResponseDto> GetTrashAsync(MessageFilterDto filter, CancellationToken cancellationToken = default);
    Task<MessageDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MessageDetailDto> ComposeAsync(ComposeMessageDto dto, CancellationToken cancellationToken = default);
    Task<MessageDetailDto> ReplyAsync(Guid id, ReplyMessageDto dto, CancellationToken cancellationToken = default);
    Task<MessageDetailDto> ToggleImportantAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MessageDetailDto> MarkReadAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task PermanentDeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MessageDetailDto> RestoreAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UnreadCountDto> GetUnreadCountAsync(CancellationToken cancellationToken = default);
    Task<MessageDetailDto> UploadAttachmentAsync(Guid id, Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RecipientLookupDto>> GetRecipientLookupAsync(string? role, Guid? classId, Guid? sectionId, CancellationToken cancellationToken = default);
}
