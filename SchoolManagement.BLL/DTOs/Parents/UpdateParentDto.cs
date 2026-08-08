namespace SchoolManagement.BLL.DTOs.Parents;

public class UpdateParentDto
{
    public string Name { get; set; } = string.Empty;
    public string? Relation { get; set; }
    public string? FatherName { get; set; }
    public string? MotherName { get; set; }
    public string? Occupation { get; set; }
    public decimal? Income { get; set; }
    public string? Education { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? MobileNo { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? AlternativeParentName { get; set; }
    public string? AlternativeParentRelation { get; set; }
    public string? AlternativeParentMobileNo { get; set; }
    public string? FacebookUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public string? LinkedInUrl { get; set; }
}
