namespace SchoolManagement.DAL.Entities.Tenant;

/// <summary>Idempotency log so cron jobs do not double-send the same reminder on the same day.</summary>
public class NotificationDispatchLog
{
    public Guid Id { get; set; }
    public string JobName { get; set; } = string.Empty;
    public string EntityKey { get; set; } = string.Empty;
    public DateOnly RunDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
