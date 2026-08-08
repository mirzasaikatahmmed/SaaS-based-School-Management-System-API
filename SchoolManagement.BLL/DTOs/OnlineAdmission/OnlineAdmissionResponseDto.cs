namespace SchoolManagement.BLL.DTOs.OnlineAdmission;

public class OnlineAdmissionResponseDto
{
    public Guid Id { get; set; }
    public int Sl { get; set; }
    public string ReferenceNo { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? BloodGroup { get; set; }
    public string? Religion { get; set; }
    public Guid? ClassId { get; set; }
    public string? ClassName { get; set; }
    public int AcademicYear { get; set; }
    public string MobileNo { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PresentAddress { get; set; }
    public string? PermanentAddress { get; set; }
    public string? BirthRegistrationNumber { get; set; }
    public string? ProfilePictureUrl { get; set; }

    public string? GuardianName { get; set; }
    public string? GuardianRelation { get; set; }
    public string? GuardianMobile { get; set; }
    public string? GuardianEmail { get; set; }
    public string? FatherName { get; set; }
    public string? MotherName { get; set; }

    public string? PreviousSchoolName { get; set; }
    public string? PreviousSchoolQualification { get; set; }

    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public decimal? PaymentAmount { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? PaymentReference { get; set; }

    public DateTime ApplyDate { get; set; }
    public string? DeclineReason { get; set; }
    public Guid? StudentId { get; set; }
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
}

public class OnlineAdmissionListResponseDto
{
    public IReadOnlyList<OnlineAdmissionResponseDto> Items { get; set; } = Array.Empty<OnlineAdmissionResponseDto>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}

public class OnlineAdmissionTrackDto
{
    public string ReferenceNo { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ClassName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public DateTime ApplyDate { get; set; }
}
