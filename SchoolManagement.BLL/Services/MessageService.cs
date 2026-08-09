using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.Message;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class MessageService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IStorageService storage,
    IHttpContextAccessor http) : IMessageService
{
    public async Task<MessageListResponseDto> GetInboxAsync(MessageFilterDto filter, CancellationToken ct = default)
    {
        await Ready(ct);
        var userId = CurrentUser();
        var (items, total) = await uow.Messages.GetInboxAsync(ToFilter(filter, userId), ct);
        return await BuildListAsync(items, total, filter, userId, ct);
    }

    public async Task<MessageListResponseDto> GetSentAsync(MessageFilterDto filter, CancellationToken ct = default)
    {
        await Ready(ct);
        var userId = CurrentUser();
        var (items, total) = await uow.Messages.GetSentAsync(ToFilter(filter, userId), ct);
        return await BuildListAsync(items, total, filter, userId, ct);
    }

    public async Task<MessageListResponseDto> GetImportantAsync(MessageFilterDto filter, CancellationToken ct = default)
    {
        await Ready(ct);
        var userId = CurrentUser();
        var (items, total) = await uow.Messages.GetImportantAsync(ToFilter(filter, userId), ct);
        return await BuildListAsync(items, total, filter, userId, ct);
    }

    public async Task<MessageListResponseDto> GetTrashAsync(MessageFilterDto filter, CancellationToken ct = default)
    {
        await Ready(ct);
        var userId = CurrentUser();
        var (items, total) = await uow.Messages.GetTrashAsync(ToFilter(filter, userId), ct);
        return await BuildListAsync(items, total, filter, userId, ct);
    }

    public async Task<MessageDetailDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        var userId = CurrentUser();
        return await BuildDetailAsync(id, userId, markAsRead: true, ct);
    }

    public async Task<MessageDetailDto> ComposeAsync(ComposeMessageDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        var senderId = CurrentUser();

        var recipientId = dto.RecipientId != Guid.Empty
            ? dto.RecipientId
            : dto.Recipients?.FirstOrDefault() ?? Guid.Empty;
        if (recipientId == Guid.Empty)
            throw new AppException("RecipientId is required.", 400);
        if (string.IsNullOrWhiteSpace(dto.Subject))
            throw new AppException("Subject is required.", 400);
        if (string.IsNullOrWhiteSpace(dto.Body))
            throw new AppException("Message body is required.", 400);

        var sender = await uow.Users.GetByIdAsync(senderId, ct)
            ?? throw new NotFoundException("Sender not found.");
        var recipient = await uow.Users.GetByIdAsync(recipientId, ct)
            ?? throw new NotFoundException("Recipient not found.");

        var message = new Message
        {
            Id = Guid.NewGuid(),
            SenderId = sender.Id,
            SenderName = FullName(sender.FirstName, sender.LastName),
            Subject = dto.Subject.Trim(),
            Body = dto.Body,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await uow.Messages.AddAsync(message, ct);

        var recipientRow = new MessageRecipient
        {
            Id = Guid.NewGuid(),
            MessageId = message.Id,
            RecipientId = recipient.Id,
            RecipientName = FullName(recipient.FirstName, recipient.LastName),
            CreatedAt = DateTime.UtcNow
        };
        await uow.Messages.AddRecipientAsync(recipientRow, ct);

        await uow.SaveTenantChangesAsync(ct);
        return await BuildDetailAsync(message.Id, senderId, markAsRead: false, ct);
    }

    public async Task<MessageDetailDto> ReplyAsync(Guid id, ReplyMessageDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        var userId = CurrentUser();
        if (string.IsNullOrWhiteSpace(dto.Body))
            throw new AppException("Reply body is required.", 400);

        var original = await uow.Messages.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Message '{id}' not found.");

        Guid recipientId;
        string recipientName;
        if (original.SenderId == userId)
        {
            var firstRecipient = original.Recipients.FirstOrDefault()
                ?? throw new AppException("Original message has no recipient to reply to.", 400);
            recipientId = firstRecipient.RecipientId;
            recipientName = firstRecipient.RecipientName;
        }
        else
        {
            _ = original.Recipients.FirstOrDefault(r => r.RecipientId == userId)
                ?? throw new ForbiddenException("You are not a participant of this message.");
            recipientId = original.SenderId;
            recipientName = original.SenderName;
        }

        var sender = await uow.Users.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("Sender not found.");

        var subject = original.Subject.StartsWith("Re: ", StringComparison.OrdinalIgnoreCase)
            ? original.Subject
            : $"Re: {original.Subject}";

        var reply = new Message
        {
            Id = Guid.NewGuid(),
            SenderId = sender.Id,
            SenderName = FullName(sender.FirstName, sender.LastName),
            ParentMessageId = original.Id,
            Subject = subject,
            Body = dto.Body,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await uow.Messages.AddAsync(reply, ct);

        var recipientRow = new MessageRecipient
        {
            Id = Guid.NewGuid(),
            MessageId = reply.Id,
            RecipientId = recipientId,
            RecipientName = recipientName,
            CreatedAt = DateTime.UtcNow
        };
        await uow.Messages.AddRecipientAsync(recipientRow, ct);

        await uow.SaveTenantChangesAsync(ct);
        return await BuildDetailAsync(reply.Id, userId, markAsRead: false, ct);
    }

    public async Task<MessageDetailDto> ToggleImportantAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        var userId = CurrentUser();
        var row = await uow.Messages.GetRecipientAsync(id, userId, ct)
            ?? throw new NotFoundException("Message not found in your mailbox.");
        row.IsImportant = !row.IsImportant;
        await uow.Messages.UpdateRecipientAsync(row, ct);
        await uow.SaveTenantChangesAsync(ct);
        return await BuildDetailAsync(id, userId, markAsRead: false, ct);
    }

    public async Task<MessageDetailDto> MarkReadAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        var userId = CurrentUser();
        return await BuildDetailAsync(id, userId, markAsRead: true, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        var userId = CurrentUser();

        var recipientRow = await uow.Messages.GetRecipientAsync(id, userId, ct);
        if (recipientRow is not null)
        {
            recipientRow.IsDeleted = true;
            recipientRow.DeletedAt = DateTime.UtcNow;
            await uow.Messages.UpdateRecipientAsync(recipientRow, ct);
            await uow.SaveTenantChangesAsync(ct);
            return;
        }

        var message = await uow.Messages.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Message '{id}' not found.");
        if (message.SenderId != userId)
            throw new ForbiddenException("You do not have access to this message.");

        message.IsDeletedBySender = true;
        message.UpdatedAt = DateTime.UtcNow;
        await uow.Messages.UpdateMessageAsync(message, ct);
        await uow.SaveTenantChangesAsync(ct);
    }

    public async Task PermanentDeleteAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        var userId = CurrentUser();

        var recipientRow = await uow.Messages.GetRecipientAsync(id, userId, ct);
        if (recipientRow is not null)
        {
            if (!recipientRow.IsDeleted)
                throw new AppException("Move the message to trash before permanently deleting it.", 400);
            await uow.Messages.DeleteRecipientAsync(recipientRow, ct);
            await uow.SaveTenantChangesAsync(ct);
            return;
        }

        var message = await uow.Messages.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Message '{id}' not found.");
        if (message.SenderId != userId)
            throw new ForbiddenException("You do not have access to this message.");
        if (!message.IsDeletedBySender)
            throw new AppException("Move the message to trash before permanently deleting it.", 400);

        await uow.Messages.DeleteMessageAsync(message, ct);
        await uow.SaveTenantChangesAsync(ct);
    }

    public async Task<MessageDetailDto> RestoreAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        var userId = CurrentUser();

        var recipientRow = await uow.Messages.GetRecipientAsync(id, userId, ct);
        if (recipientRow is not null)
        {
            recipientRow.IsDeleted = false;
            recipientRow.DeletedAt = null;
            await uow.Messages.UpdateRecipientAsync(recipientRow, ct);
            await uow.SaveTenantChangesAsync(ct);
            return await BuildDetailAsync(id, userId, markAsRead: false, ct);
        }

        var message = await uow.Messages.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Message '{id}' not found.");
        if (message.SenderId != userId)
            throw new ForbiddenException("You do not have access to this message.");

        message.IsDeletedBySender = false;
        message.UpdatedAt = DateTime.UtcNow;
        await uow.Messages.UpdateMessageAsync(message, ct);
        await uow.SaveTenantChangesAsync(ct);
        return await BuildDetailAsync(id, userId, markAsRead: false, ct);
    }

    public async Task<UnreadCountDto> GetUnreadCountAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        var userId = CurrentUser();
        return new UnreadCountDto { Count = await uow.Messages.GetUnreadCountAsync(userId, ct) };
    }

    public async Task<MessageDetailDto> UploadAttachmentAsync(
        Guid id, Stream stream, string fileName, string contentType, CancellationToken ct = default)
    {
        await Ready(ct);
        var userId = CurrentUser();
        var message = await uow.Messages.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Message '{id}' not found.");
        if (message.SenderId != userId)
            throw new ForbiddenException("Only the sender can attach a file to this message.");

        var slug = tenant.TenantSlug ?? throw new AppException("X-Tenant-ID header is required.", 400);
        var objectKey = await storage.UploadFileAsync(slug, AppConstants.StorageFolders.Messages, stream, fileName, contentType, ct);

        message.AttachmentUrl = objectKey;
        message.AttachmentName = fileName;
        message.UpdatedAt = DateTime.UtcNow;
        await uow.Messages.UpdateMessageAsync(message, ct);
        await uow.SaveTenantChangesAsync(ct);
        return await BuildDetailAsync(id, userId, markAsRead: false, ct);
    }

    public async Task<IReadOnlyList<RecipientLookupDto>> GetRecipientLookupAsync(
        string? role, Guid? classId, Guid? sectionId, CancellationToken ct = default)
    {
        await Ready(ct);
        var results = new List<RecipientLookupDto>();

        var wantsStudents = string.IsNullOrWhiteSpace(role) || AwardRoles.IsStudent(role);
        var wantsEmployees = string.IsNullOrWhiteSpace(role) || !AwardRoles.IsStudent(role);

        if (wantsStudents)
        {
            var (students, _) = await uow.Students.SearchAsync(new StudentSearchFilter
            {
                ClassId = classId,
                SectionId = sectionId,
                IsActive = true,
                Page = 1,
                PageSize = 500
            }, ct);
            results.AddRange(students.Select(s => new RecipientLookupDto
            {
                Id = s.UserId,
                DisplayName = FullName(s.FirstName, s.LastName),
                RecipientType = "Student",
                SubText = s.RegisterNo
            }));
        }

        if (wantsEmployees)
        {
            var employeeRole = string.IsNullOrWhiteSpace(role) || AwardRoles.IsStudent(role) ? null : role!.Trim();
            var (employees, _) = await uow.Employees.SearchAsync(new EmployeeSearchFilter
            {
                Role = employeeRole,
                IsActive = true,
                Page = 1,
                PageSize = 500
            }, ct);
            results.AddRange(employees.Select(e => new RecipientLookupDto
            {
                Id = e.UserId,
                DisplayName = e.Name,
                RecipientType = e.Role,
                SubText = e.StaffId
            }));
        }

        return results;
    }

    private async Task<MessageListResponseDto> BuildListAsync(
        IReadOnlyList<MessageMailboxItem> items, int total, MessageFilterDto filter, Guid userId, CancellationToken ct)
    {
        var page = filter.Page < 1 ? 1 : filter.Page;
        var size = filter.PageSize is < 1 or > 200 ? 25 : filter.PageSize;
        var unread = await uow.Messages.GetUnreadCountAsync(userId, ct);

        return new MessageListResponseDto
        {
            Data = items.Select((x, i) => new MessageListItemDto
            {
                Id = x.MessageId,
                Sl = (page - 1) * size + i + 1,
                SenderName = x.SenderName,
                RecipientName = x.RecipientName,
                Subject = x.Subject,
                PreviewText = StripHtml(x.Body),
                IsRead = x.IsRead,
                IsImportant = x.IsImportant,
                HasAttachment = !string.IsNullOrWhiteSpace(x.AttachmentUrl),
                CreatedAt = x.CreatedAt
            }).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = size,
            TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)size),
            UnreadCount = unread
        };
    }

    private async Task<MessageDetailDto> BuildDetailAsync(Guid messageId, Guid viewerId, bool markAsRead, CancellationToken ct)
    {
        var message = await uow.Messages.GetByIdAsync(messageId, ct)
            ?? throw new NotFoundException($"Message '{messageId}' not found.");

        var viewerRow = message.Recipients.FirstOrDefault(r => r.RecipientId == viewerId);
        if (message.SenderId != viewerId && viewerRow is null)
            throw new ForbiddenException("You do not have access to this message.");

        if (markAsRead && viewerRow is not null && !viewerRow.IsRead)
        {
            viewerRow.IsRead = true;
            viewerRow.ReadAt = DateTime.UtcNow;
            await uow.Messages.UpdateRecipientAsync(viewerRow, ct);
            await uow.SaveTenantChangesAsync(ct);
        }

        var primaryRecipient = message.Recipients.FirstOrDefault();
        string? attachmentUrl = message.AttachmentUrl;
        if (!string.IsNullOrWhiteSpace(attachmentUrl) && !string.IsNullOrWhiteSpace(tenant.TenantSlug))
        {
            try
            {
                attachmentUrl = await storage.GetPresignedUrlAsync(tenant.TenantSlug!, attachmentUrl, ct);
            }
            catch
            {
                // leave raw object key if presign fails
            }
        }

        return new MessageDetailDto
        {
            Id = message.Id,
            SenderId = message.SenderId,
            SenderName = message.SenderName,
            RecipientId = primaryRecipient?.RecipientId,
            RecipientName = primaryRecipient?.RecipientName,
            Subject = message.Subject,
            Body = message.Body,
            AttachmentUrl = attachmentUrl,
            AttachmentName = message.AttachmentName,
            IsRead = viewerRow?.IsRead ?? true,
            IsImportant = viewerRow?.IsImportant ?? false,
            ParentMessageId = message.ParentMessageId,
            CreatedAt = message.CreatedAt
        };
    }

    private static MessageFilter ToFilter(MessageFilterDto dto, Guid userId) => new()
    {
        UserId = userId,
        Search = dto.Search,
        Page = dto.Page,
        PageSize = dto.PageSize
    };

    private static string FullName(string firstName, string? lastName)
        => string.IsNullOrWhiteSpace(lastName) ? firstName.Trim() : $"{firstName.Trim()} {lastName.Trim()}";

    private static string StripHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;
        var text = Regex.Replace(html, "<.*?>", " ");
        text = WebUtility.HtmlDecode(text);
        text = Regex.Replace(text, @"\s+", " ").Trim();
        return text.Length > 160 ? text[..160] + "..." : text;
    }

    private async Task Ready(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(tenant.SchemaName))
            throw new AppException("X-Tenant-ID header is required.", 400);
        await provisioner.EnsureMessageAndSettingsModuleAsync(tenant.SchemaName!, ct);
    }

    private Guid CurrentUser()
    {
        var claim = http.HttpContext?.User.FindFirst(AppConstants.Claims.UserId)
            ?? http.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (claim is null || !Guid.TryParse(claim.Value, out var id))
            throw new UnauthorizedException();
        return id;
    }
}
