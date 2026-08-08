namespace SchoolManagement.BLL.DTOs.Import;

public class ImportResultDto
{
    public Guid BatchId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<ImportRowResultDto> FailedRows { get; set; } = new();
}
