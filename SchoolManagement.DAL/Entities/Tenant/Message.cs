namespace SchoolManagement.DAL.Entities.Tenant;

public class Message
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public Guid? ParentMessageId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
    public string? AttachmentName { get; set; }
    public bool IsDeletedBySender { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User Sender { get; set; } = null!;
    public Message? ParentMessage { get; set; }
    public ICollection<MessageRecipient> Recipients { get; set; } = new List<MessageRecipient>();
}

public class MessageRecipient
{
    public Guid Id { get; set; }
    public Guid MessageId { get; set; }
    public Guid RecipientId { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public bool IsImportant { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Message Message { get; set; } = null!;
    public User Recipient { get; set; } = null!;
}
