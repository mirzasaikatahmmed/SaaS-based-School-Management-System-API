namespace SchoolManagement.BLL.DTOs.Student;

public class UpdateAdmissionDto
{
    public string? Roll { get; set; }
    public DateTime? AdmissionDate { get; set; }
    public Guid? ClassId { get; set; }
    public Guid? SectionId { get; set; }
    public Guid? CategoryId { get; set; }

    public string? FirstName { get; set; }
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
    public string? FathersNidNumber { get; set; }
    public string? MothersNidNumber { get; set; }
    public string? BirthRegistrationNumber { get; set; }

    public Guid? TransportRouteId { get; set; }
    public string? VehicleNo { get; set; }
    public Guid? HostelId { get; set; }
    public Guid? RoomId { get; set; }

    public string? PreviousSchoolName { get; set; }
    public string? PreviousSchoolQualification { get; set; }
    public string? Remarks { get; set; }

    public GuardianDto? Guardian { get; set; }
}
