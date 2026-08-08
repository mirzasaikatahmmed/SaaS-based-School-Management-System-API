namespace SchoolManagement.DAL.Entities.Tenant;

public class ImportBatchRow
{
    public Guid Id { get; set; }
    public Guid BatchId { get; set; }
    public int RowNumber { get; set; }
    public string RawData { get; set; } = "{}";
    public string Status { get; set; } = ImportBatchRowStatuses.Pending;
    public Guid? StudentId { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ImportBatch Batch { get; set; } = null!;
    public Student? Student { get; set; }
}

public static class ImportBatchRowStatuses
{
    public const string Pending = "Pending";
    public const string Success = "Success";
    public const string Failed = "Failed";
}
