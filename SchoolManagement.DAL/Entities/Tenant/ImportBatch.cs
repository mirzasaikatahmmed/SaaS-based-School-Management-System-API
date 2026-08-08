namespace SchoolManagement.DAL.Entities.Tenant;

public class ImportBatch
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public string Status { get; set; } = ImportBatchStatuses.Processing;
    public Guid ImportedBy { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ClassEntity? Class { get; set; }
    public Section? Section { get; set; }
    public ICollection<ImportBatchRow> Rows { get; set; } = new List<ImportBatchRow>();
}

public static class ImportBatchStatuses
{
    public const string Processing = "Processing";
    public const string Completed = "Completed";
    public const string CompletedWithErrors = "CompletedWithErrors";
    public const string Failed = "Failed";
}
