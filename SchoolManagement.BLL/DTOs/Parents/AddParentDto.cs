namespace SchoolManagement.BLL.DTOs.Parents;

public class AddParentDto
{
    public string Name { get; set; } = string.Empty;
    public string Relation { get; set; } = string.Empty;
    public string? FatherName { get; set; }
    public string? MotherName { get; set; }
    public string Occupation { get; set; } = string.Empty;
    public decimal? Income { get; set; }
    public string? Education { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string MobileNo { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }

    public string? AlternativeParentName { get; set; }
    public string? AlternativeParentRelation { get; set; }
    public string? AlternativeParentMobileNo { get; set; }

    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string RetypePassword { get; set; } = string.Empty;

    public string? FacebookUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public string? LinkedInUrl { get; set; }

    public List<Guid>? StudentIds { get; set; }
}
