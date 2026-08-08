namespace SchoolManagement.DAL.Entities.Tenant;

public class Student
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string RegisterNo { get; set; } = string.Empty;
    public string? Roll { get; set; }
    public int AcademicYear { get; set; }
    public DateTime AdmissionDate { get; set; } = DateTime.UtcNow.Date;
    public Guid? ClassId { get; set; }
    public Guid? SectionId { get; set; }
    public Guid? CategoryId { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string? Gender { get; set; }
    public string? BloodGroup { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? MotherTongue { get; set; }
    public string? Religion { get; set; }
    public string? Caste { get; set; }
    public string? MobileNo { get; set; }
    public string? Email { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PresentAddress { get; set; }
    public string? PermanentAddress { get; set; }
    public string? ProfilePictureUrl { get; set; }

    public string? FathersNidNumber { get; set; }
    public string? MothersNidNumber { get; set; }
    public string? BirthRegistrationNumber { get; set; }

    public string? PreviousSchoolName { get; set; }
    public string? PreviousSchoolQualification { get; set; }
    public string? Remarks { get; set; }

    public Guid? TransportRouteId { get; set; }
    public string? VehicleNo { get; set; }

    public Guid? HostelId { get; set; }
    public Guid? RoomId { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public ClassEntity? Class { get; set; }
    public Section? Section { get; set; }
    public StudentCategory? Category { get; set; }
    public TransportRoute? TransportRoute { get; set; }
    public Hostel? Hostel { get; set; }
    public HostelRoom? Room { get; set; }
    public ICollection<Guardian> Guardians { get; set; } = new List<Guardian>();
}
