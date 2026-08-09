namespace SchoolManagement.DAL.Entities.Tenant;

/// <summary>
/// Subject / period attendance (SubjectWise mode). Day-wise class attendance stays in
/// <see cref="StudentAttendance"/>.
/// </summary>
public class StudentSubjectAttendance
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public Guid SubjectId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public string Status { get; set; } = "Present";
    public string? Remarks { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Student Student { get; set; } = null!;
    public ClassEntity Class { get; set; } = null!;
    public Section Section { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
    public User? CreatedByUser { get; set; }
}
