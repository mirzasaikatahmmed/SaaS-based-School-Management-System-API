namespace SchoolManagement.BLL.DTOs.Import;

public class ImportRowResultDto
{
    public int RowNumber { get; set; }
    public string? RegisterNo { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, string> RawData { get; set; } = new();
}
