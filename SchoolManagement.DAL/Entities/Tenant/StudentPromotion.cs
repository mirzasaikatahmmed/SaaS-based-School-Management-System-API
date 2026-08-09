namespace SchoolManagement.DAL.Entities.Tenant;

public class StudentPromotion
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public int FromAcademicYear { get; set; }
    public Guid? FromClassId { get; set; }
    public Guid? FromSectionId { get; set; }
    public string? FromRoll { get; set; }
    public int ToAcademicYear { get; set; }
    public Guid? ToClassId { get; set; }
    public Guid? ToSectionId { get; set; }
    public string? ToRoll { get; set; }
    public string Status { get; set; } = PromotionStatuses.Promoted;
    public decimal CurrentDueAmount { get; set; }
    public bool CarryForwardDue { get; set; } = true;
    public Guid? PromotedBy { get; set; }
    public DateTime PromotedAt { get; set; } = DateTime.UtcNow;

    public Student Student { get; set; } = null!;
    public ClassEntity? FromClass { get; set; }
    public Section? FromSection { get; set; }
    public ClassEntity? ToClass { get; set; }
    public Section? ToSection { get; set; }
    public User? PromotedByUser { get; set; }
}

public static class PromotionStatuses
{
    public const string Promoted = "Promoted";
    public const string Running = "Running";
    public const string Left = "Left";
    public const string Alumni = "Alumni";
    public static readonly string[] All = [Promoted, Running, Left, Alumni];
}
