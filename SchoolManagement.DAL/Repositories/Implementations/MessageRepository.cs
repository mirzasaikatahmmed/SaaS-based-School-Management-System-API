using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class MessageRepository(TenantDbContext context) : IMessageRepository
{
    public async Task<Message?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Recipients).ThenInclude(r => r.Recipient)
            .Include(m => m.ParentMessage)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task<MessageRecipient?> GetRecipientAsync(Guid messageId, Guid userId, CancellationToken cancellationToken = default)
        => await context.MessageRecipients
            .Include(r => r.Message)
            .FirstOrDefaultAsync(r => r.MessageId == messageId && r.RecipientId == userId, cancellationToken);

    public async Task<MessageRecipient?> GetRecipientByIdAsync(Guid recipientRowId, CancellationToken cancellationToken = default)
        => await context.MessageRecipients
            .Include(r => r.Message)
            .FirstOrDefaultAsync(r => r.Id == recipientRowId, cancellationToken);

    public async Task<(IReadOnlyList<MessageMailboxItem> Items, int TotalCount)> GetInboxAsync(
        MessageFilter filter, CancellationToken cancellationToken = default)
    {
        var q = context.MessageRecipients
            .Include(r => r.Message)
            .Where(r => r.RecipientId == filter.UserId && !r.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            q = q.Where(r => r.Message.Subject.ToLower().Contains(s) || r.Message.SenderName.ToLower().Contains(s));
        }

        var total = await q.CountAsync(cancellationToken);
        var (page, size) = Normalize(filter);
        var rows = await q.OrderByDescending(r => r.Message.CreatedAt)
            .Skip((page - 1) * size).Take(size)
            .ToListAsync(cancellationToken);
        return (rows.Select(ToItem).ToList(), total);
    }

    public async Task<(IReadOnlyList<MessageMailboxItem> Items, int TotalCount)> GetSentAsync(
        MessageFilter filter, CancellationToken cancellationToken = default)
    {
        var q = context.Messages
            .Include(m => m.Recipients)
            .Where(m => m.SenderId == filter.UserId && !m.IsDeletedBySender)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            q = q.Where(m => m.Subject.ToLower().Contains(s));
        }

        var total = await q.CountAsync(cancellationToken);
        var (page, size) = Normalize(filter);
        var rows = await q.OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * size).Take(size)
            .ToListAsync(cancellationToken);
        return (rows.Select(ToItem).ToList(), total);
    }

    public async Task<(IReadOnlyList<MessageMailboxItem> Items, int TotalCount)> GetImportantAsync(
        MessageFilter filter, CancellationToken cancellationToken = default)
    {
        var q = context.MessageRecipients
            .Include(r => r.Message)
            .Where(r => r.RecipientId == filter.UserId && r.IsImportant && !r.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            q = q.Where(r => r.Message.Subject.ToLower().Contains(s) || r.Message.SenderName.ToLower().Contains(s));
        }

        var total = await q.CountAsync(cancellationToken);
        var (page, size) = Normalize(filter);
        var rows = await q.OrderByDescending(r => r.Message.CreatedAt)
            .Skip((page - 1) * size).Take(size)
            .ToListAsync(cancellationToken);
        return (rows.Select(ToItem).ToList(), total);
    }

    public async Task<(IReadOnlyList<MessageMailboxItem> Items, int TotalCount)> GetTrashAsync(
        MessageFilter filter, CancellationToken cancellationToken = default)
    {
        var receivedQ = context.MessageRecipients
            .Include(r => r.Message)
            .Where(r => r.RecipientId == filter.UserId && r.IsDeleted)
            .AsQueryable();

        var sentQ = context.Messages
            .Include(m => m.Recipients)
            .Where(m => m.SenderId == filter.UserId && m.IsDeletedBySender)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            receivedQ = receivedQ.Where(r => r.Message.Subject.ToLower().Contains(s) || r.Message.SenderName.ToLower().Contains(s));
            sentQ = sentQ.Where(m => m.Subject.ToLower().Contains(s));
        }

        var receivedRows = await receivedQ.OrderByDescending(r => r.Message.CreatedAt).Take(2000)
            .ToListAsync(cancellationToken);
        var sentRows = await sentQ.OrderByDescending(m => m.CreatedAt).Take(2000)
            .ToListAsync(cancellationToken);

        var combined = receivedRows.Select(ToItem).Concat(sentRows.Select(ToItem))
            .OrderByDescending(x => x.CreatedAt).ToList();
        var total = combined.Count;
        var (page, size) = Normalize(filter);
        var items = combined.Skip((page - 1) * size).Take(size).ToList();
        return (items, total);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
        => await context.MessageRecipients.CountAsync(r => r.RecipientId == userId && !r.IsRead && !r.IsDeleted, cancellationToken);

    public async Task<Message> AddAsync(Message message, CancellationToken cancellationToken = default)
    {
        await context.Messages.AddAsync(message, cancellationToken);
        return message;
    }

    public async Task<MessageRecipient> AddRecipientAsync(MessageRecipient recipient, CancellationToken cancellationToken = default)
    {
        await context.MessageRecipients.AddAsync(recipient, cancellationToken);
        return recipient;
    }

    public Task UpdateMessageAsync(Message message, CancellationToken cancellationToken = default)
    {
        context.Messages.Update(message);
        return Task.CompletedTask;
    }

    public Task UpdateRecipientAsync(MessageRecipient recipient, CancellationToken cancellationToken = default)
    {
        context.MessageRecipients.Update(recipient);
        return Task.CompletedTask;
    }

    public Task DeleteMessageAsync(Message message, CancellationToken cancellationToken = default)
    {
        context.Messages.Remove(message);
        return Task.CompletedTask;
    }

    public Task DeleteRecipientAsync(MessageRecipient recipient, CancellationToken cancellationToken = default)
    {
        context.MessageRecipients.Remove(recipient);
        return Task.CompletedTask;
    }

    private static (int Page, int Size) Normalize(MessageFilter filter)
    {
        var page = filter.Page < 1 ? 1 : filter.Page;
        var size = filter.PageSize is < 1 or > 200 ? 25 : filter.PageSize;
        return (page, size);
    }

    private static MessageMailboxItem ToItem(MessageRecipient r) => new()
    {
        MessageId = r.MessageId,
        RecipientRowId = r.Id,
        SenderId = r.Message.SenderId,
        SenderName = r.Message.SenderName,
        RecipientId = r.RecipientId,
        RecipientName = r.RecipientName,
        Subject = r.Message.Subject,
        Body = r.Message.Body,
        AttachmentUrl = r.Message.AttachmentUrl,
        AttachmentName = r.Message.AttachmentName,
        IsRead = r.IsRead,
        IsImportant = r.IsImportant,
        ParentMessageId = r.Message.ParentMessageId,
        CreatedAt = r.Message.CreatedAt
    };

    private static MessageMailboxItem ToItem(Message m) => new()
    {
        MessageId = m.Id,
        RecipientRowId = null,
        SenderId = m.SenderId,
        SenderName = m.SenderName,
        RecipientId = m.Recipients.Select(r => (Guid?)r.RecipientId).FirstOrDefault(),
        RecipientName = m.Recipients.Select(r => r.RecipientName).FirstOrDefault(),
        Subject = m.Subject,
        Body = m.Body,
        AttachmentUrl = m.AttachmentUrl,
        AttachmentName = m.AttachmentName,
        IsRead = true,
        IsImportant = false,
        ParentMessageId = m.ParentMessageId,
        CreatedAt = m.CreatedAt
    };
}
