using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public class MessageFilter
{
    public Guid UserId { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

/// <summary>Unified mailbox row — projected from either the Message (sent) or MessageRecipient (received) side.</summary>
public class MessageMailboxItem
{
    public Guid MessageId { get; set; }
    public Guid? RecipientRowId { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public Guid? RecipientId { get; set; }
    public string? RecipientName { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
    public string? AttachmentName { get; set; }
    public bool IsRead { get; set; }
    public bool IsImportant { get; set; }
    public Guid? ParentMessageId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public interface IMessageRepository
{
    Task<Message?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MessageRecipient?> GetRecipientAsync(Guid messageId, Guid userId, CancellationToken cancellationToken = default);
    Task<MessageRecipient?> GetRecipientByIdAsync(Guid recipientRowId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<MessageMailboxItem> Items, int TotalCount)> GetInboxAsync(MessageFilter filter, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<MessageMailboxItem> Items, int TotalCount)> GetSentAsync(MessageFilter filter, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<MessageMailboxItem> Items, int TotalCount)> GetImportantAsync(MessageFilter filter, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<MessageMailboxItem> Items, int TotalCount)> GetTrashAsync(MessageFilter filter, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Message> AddAsync(Message message, CancellationToken cancellationToken = default);
    Task<MessageRecipient> AddRecipientAsync(MessageRecipient recipient, CancellationToken cancellationToken = default);
    Task UpdateMessageAsync(Message message, CancellationToken cancellationToken = default);
    Task UpdateRecipientAsync(MessageRecipient recipient, CancellationToken cancellationToken = default);
    Task DeleteMessageAsync(Message message, CancellationToken cancellationToken = default);
    Task DeleteRecipientAsync(MessageRecipient recipient, CancellationToken cancellationToken = default);
}
