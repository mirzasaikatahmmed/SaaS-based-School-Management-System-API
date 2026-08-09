namespace SchoolManagement.DAL.Entities.Tenant;

public class ExamHall
{
    public Guid Id { get; set; }
    public string HallNo { get; set; } = string.Empty;
    public int NoOfSeats { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
