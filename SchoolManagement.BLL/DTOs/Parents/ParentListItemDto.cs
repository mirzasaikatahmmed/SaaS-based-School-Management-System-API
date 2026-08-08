namespace SchoolManagement.BLL.DTOs.Parents;

public class ParentListItemDto
{
    public Guid Id { get; set; }
    public int Sl { get; set; }
    public string Branch { get; set; } = string.Empty;
    public string GuardianName { get; set; } = string.Empty;
    public string? Occupation { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Email { get; set; }
    public bool IsLoginActive { get; set; }
}
