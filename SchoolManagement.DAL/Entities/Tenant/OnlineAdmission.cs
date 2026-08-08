namespace SchoolManagement.DAL.Entities.Tenant;

public class OnlineAdmission
{
    public Guid Id { get; set; }
    public string ReferenceNo { get; set; } = string.Empty;

    public int AcademicYear { get; set; }
    public Guid? ClassId { get; set; }
    public string? ClassName { get; set; }

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
    public string? ProfilePictureUrl { get; set; }

    public string? GuardianName { get; set; }
    public string? GuardianRelation { get; set; }
    public string? GuardianMobile { get; set; }
    public string? GuardianEmail { get; set; }
    public string? FatherName { get; set; }
    public string? MotherName { get; set; }

    public string? PreviousSchoolName { get; set; }
    public string? PreviousSchoolQualification { get; set; }

    /// <summary>Apply | Approved | Declined</summary>
    public string Status { get; set; } = OnlineAdmissionStatuses.Apply;

    /// <summary>Unpaid | Paid</summary>
    public string PaymentStatus { get; set; } = OnlineAdmissionPaymentStatuses.Unpaid;

    public decimal? PaymentAmount { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? PaymentReference { get; set; }

    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? DeclineReason { get; set; }

    public Guid? StudentId { get; set; }

    public DateTime ApplyDate { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ClassEntity? Class { get; set; }
    public Student? Student { get; set; }
}

public static class OnlineAdmissionStatuses
{
    public const string Apply = "Apply";
    public const string Approved = "Approved";
    public const string Declined = "Declined";
}

public static class OnlineAdmissionPaymentStatuses
{
    public const string Unpaid = "Unpaid";
    public const string Paid = "Paid";
}
