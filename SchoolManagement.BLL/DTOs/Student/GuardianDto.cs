namespace SchoolManagement.BLL.DTOs.Student;

public class GuardianDto
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Relation { get; set; } = string.Empty;
    public string? FatherName { get; set; }
    public string? MotherName { get; set; }
    public string? Occupation { get; set; }
    public decimal? Income { get; set; }
    public string? Education { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string MobileNo { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public bool IsPrimary { get; set; } = true;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? RetypePassword { get; set; }
}
