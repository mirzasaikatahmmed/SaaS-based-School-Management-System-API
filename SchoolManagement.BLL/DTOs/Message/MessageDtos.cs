namespace SchoolManagement.BLL.DTOs.Message;

public class ComposeMessageDto
{
    public Guid RecipientId { get; set; }

    /// <summary>Reserved for future multi-recipient support — current API uses RecipientId.</summary>
    public List<Guid>? Recipients { get; set; }

    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}

public class ReplyMessageDto
{
    public string Body { get; set; } = string.Empty;
}

public class MessageFilterDto
{
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public class MessageListItemDto
{
    public Guid Id { get; set; }
    public int Sl { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string? RecipientName { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string PreviewText { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public bool IsImportant { get; set; }
    public bool HasAttachment { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MessageDetailDto
{
    public Guid Id { get; set; }
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

public class MessageListResponseDto
{
    public IReadOnlyList<MessageListItemDto> Data { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int UnreadCount { get; set; }
}

public class UnreadCountDto
{
    public int Count { get; set; }
}

public class RecipientLookupDto
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string RecipientType { get; set; } = string.Empty;
    public string? SubText { get; set; }
}
