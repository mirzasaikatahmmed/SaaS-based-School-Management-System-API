namespace SchoolManagement.DAL.Entities.Tenant;

public class Award
{
    public Guid Id { get; set; }
    public Guid? EmployeeId { get; set; }
    public Guid? StudentId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string AwardName { get; set; } = string.Empty;
    public string GiftItem { get; set; } = string.Empty;
    public decimal? CashPrice { get; set; }
    public string AwardReason { get; set; } = string.Empty;
    public DateTime GivenDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Employee? Employee { get; set; }
    public Student? Student { get; set; }
}

public static class AwardRoles
{
    public const string Student = "Student";

    public static readonly string[] All =
    [
        EmployeeRoles.Admin,
        EmployeeRoles.Teacher,
        EmployeeRoles.Accountant,
        EmployeeRoles.Librarian,
        EmployeeRoles.Receptionist,
        EmployeeRoles.Staff,
        EmployeeRoles.Demo,
        Student
    ];

    public static bool IsStudent(string? role) =>
        string.Equals(role?.Trim(), Student, StringComparison.OrdinalIgnoreCase);
}
