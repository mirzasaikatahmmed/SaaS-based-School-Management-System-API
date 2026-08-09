namespace SchoolManagement.BLL.DTOs.Employee;

public class AddEmployeeDto
{
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
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string RetypePassword { get; set; } = string.Empty;
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
}
