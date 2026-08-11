namespace SchoolManagement.BLL.DTOs.Student;

public class CreateAdmissionDto
{
    public int AcademicYear { get; set; }
    public string RegisterNo { get; set; } = string.Empty;
    public string? Roll { get; set; }
    /// <summary>Optional board SSC roll — only for class 9 / 10; omit or leave blank if unknown.</summary>
    public string? SscRoll { get; set; }
    /// <summary>Optional board SSC registration number — only for class 9 / 10; omit or leave blank if unknown.</summary>
    public string? SscRegistrationNo { get; set; }
    public DateTime AdmissionDate { get; set; } = DateTime.UtcNow.Date;
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public Guid? CategoryId { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string? Gender { get; set; }
    public string? BloodGroup { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? MotherTongue { get; set; }
    public string Religion { get; set; } = string.Empty;
    public string? Caste { get; set; }
    public string MobileNo { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PresentAddress { get; set; }
    public string? PermanentAddress { get; set; }
    public string? FathersNidNumber { get; set; }
    public string? MothersNidNumber { get; set; }
    public string? BirthRegistrationNumber { get; set; }

    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string RetypePassword { get; set; } = string.Empty;

    public bool GuardianAlreadyExist { get; set; } = false;
    public Guid? ExistingGuardianId { get; set; }
    public GuardianDto? Guardian { get; set; }

    public Guid? TransportRouteId { get; set; }
    public string? VehicleNo { get; set; }

    public Guid? HostelId { get; set; }
    public Guid? RoomId { get; set; }

    public string? PreviousSchoolName { get; set; }
    public string? PreviousSchoolQualification { get; set; }
    public string? Remarks { get; set; }
}
