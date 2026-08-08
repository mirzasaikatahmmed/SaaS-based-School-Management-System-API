namespace SchoolManagement.DAL.Entities.Tenant;

public class Guardian
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Guid? UserId { get; set; }
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
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Student Student { get; set; } = null!;
    public User? User { get; set; }
}
