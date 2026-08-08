namespace SchoolManagement.BLL.DTOs.StudentList;

public class StudentDetailDto
{
    public Guid Id { get; set; }
    public string RegisterNo { get; set; } = string.Empty;
    public string? Roll { get; set; }
    public int AcademicYear { get; set; }
    public DateTime AdmissionDate { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string? Gender { get; set; }
    public string? BloodGroup { get; set; }
    public string? DateOfBirth { get; set; }
    public int? Age { get; set; }
    public string? MotherTongue { get; set; }
    public string? Religion { get; set; }
    public string? Caste { get; set; }
    public string? MobileNo { get; set; }
    public string? Email { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PresentAddress { get; set; }
    public string? PermanentAddress { get; set; }
    public string? FathersNidNumber { get; set; }
    public string? MothersNidNumber { get; set; }
    public string? BirthRegistrationNumber { get; set; }
    public string? PhotoUrl { get; set; }

    public string Username { get; set; } = string.Empty;
    public bool IsLoginActive { get; set; }

    public List<GuardianDetailDto> Guardians { get; set; } = new();

    public string? TransportRoute { get; set; }
    public string? VehicleNo { get; set; }

    public string? HostelName { get; set; }
    public string? RoomName { get; set; }

    public string? PreviousSchoolName { get; set; }
    public string? PreviousSchoolQualification { get; set; }
    public string? Remarks { get; set; }

    public bool IsActive { get; set; }
    public string? DeactivateReason { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GuardianDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Relation { get; set; } = string.Empty;
    public string? FatherName { get; set; }
    public string? MotherName { get; set; }
    public string? MobileNo { get; set; }
    public string? Email { get; set; }
    public string? Occupation { get; set; }
    public string? Address { get; set; }
    public string? PhotoUrl { get; set; }
    public bool IsPrimary { get; set; }
}
