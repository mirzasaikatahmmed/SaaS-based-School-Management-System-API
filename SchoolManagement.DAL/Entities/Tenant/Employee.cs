namespace SchoolManagement.DAL.Entities.Tenant;

public class Employee
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string StaffId { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;
    public Guid? DesignationId { get; set; }
    public Guid? DepartmentId { get; set; }
    public DateTime JoiningDate { get; set; }
    public string? Qualification { get; set; }
    public string? ExperienceDetails { get; set; }
    public string? TotalExperience { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Gender { get; set; }
    public string? Religion { get; set; }
    public string? BloodGroup { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string MobileNo { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PresentAddress { get; set; }
    public string? PermanentAddress { get; set; }
    public string? NidNumber { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? SignatureUrl { get; set; }

    public string? FacebookUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public string? LinkedInUrl { get; set; }

    public bool SkipBankDetails { get; set; }
    public string? BankName { get; set; }
    public string? HolderName { get; set; }
    public string? BankBranch { get; set; }
    public string? BankAddress { get; set; }
    public string? IfscCode { get; set; }
    public string? AccountNo { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Designation? Designation { get; set; }
    public Department? Department { get; set; }
}

public class EmployeeImportBatch
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public string Status { get; set; } = ImportBatchStatuses.Processing;
    public Guid ImportedBy { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<EmployeeImportBatchRow> Rows { get; set; } = new List<EmployeeImportBatchRow>();
}

public class EmployeeImportBatchRow
{
    public Guid Id { get; set; }
    public Guid BatchId { get; set; }
    public int RowNumber { get; set; }
    public string RawData { get; set; } = "{}";
    public string Status { get; set; } = "Pending";
    public Guid? EmployeeId { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public EmployeeImportBatch Batch { get; set; } = null!;
    public Employee? Employee { get; set; }
}

public static class EmployeeRoles
{
    public const string Admin = "Admin";
    public const string Teacher = "Teacher";
    public const string Accountant = "Accountant";
    public const string Librarian = "Librarian";
    public const string Receptionist = "Receptionist";
    public const string Staff = "Staff";
    public const string Demo = "Demo";

    public static readonly string[] All =
    [
        Admin, Teacher, Accountant, Librarian, Receptionist, Staff, Demo
    ];
}
