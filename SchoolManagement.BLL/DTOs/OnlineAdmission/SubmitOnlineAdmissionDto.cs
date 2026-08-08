namespace SchoolManagement.BLL.DTOs.OnlineAdmission;

public class SubmitOnlineAdmissionDto
{
    public string TenantSlug { get; set; } = string.Empty;
    public int AcademicYear { get; set; }
    public Guid ClassId { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? BloodGroup { get; set; }
    public string? Religion { get; set; }
    public string MobileNo { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PresentAddress { get; set; }
    public string? PermanentAddress { get; set; }
    public string? BirthRegistrationNumber { get; set; }

    public string? GuardianName { get; set; }
    public string? GuardianRelation { get; set; }
    public string? GuardianMobile { get; set; }
    public string? GuardianEmail { get; set; }
    public string? FatherName { get; set; }
    public string? MotherName { get; set; }

    public string? PreviousSchoolName { get; set; }
    public string? PreviousSchoolQualification { get; set; }
}
