namespace SchoolManagement.BLL.DTOs.Parents;

public class ParentDetailDto
{
    public Guid Id { get; set; }
    public string? ReferenceNo { get; set; }
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
    public string? PhotoUrl { get; set; }
    public string? Username { get; set; }
    public bool IsLoginActive { get; set; }

    public string? AlternativeParentName { get; set; }
    public string? AlternativeParentRelation { get; set; }
    public string? AlternativeParentMobileNo { get; set; }
    public string? FacebookUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public string? LinkedInUrl { get; set; }

    public List<LinkedStudentDto> Students { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class LinkedStudentDto
{
    public Guid StudentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RegisterNo { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
}
