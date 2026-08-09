namespace SchoolManagement.DAL.Entities.Tenant;

public class BookIssue
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public string Role { get; set; } = string.Empty;
    public Guid? StudentId { get; set; }
    public Guid? EmployeeId { get; set; }
    public string? UserName { get; set; }
    public DateTime DateOfIssue { get; set; } = DateTime.UtcNow.Date;
    public DateTime DateOfExpiry { get; set; }
    public DateTime? ReturnDate { get; set; }
    public decimal Fine { get; set; }
    public string Status { get; set; } = "Issued";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Book Book { get; set; } = null!;
    public Student? Student { get; set; }
    public Employee? Employee { get; set; }
}
