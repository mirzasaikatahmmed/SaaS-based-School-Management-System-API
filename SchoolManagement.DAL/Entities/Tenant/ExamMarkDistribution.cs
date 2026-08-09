namespace SchoolManagement.DAL.Entities.Tenant;

public class ExamMarkDistribution
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public Guid MarkDistributionId { get; set; }

    public Exam Exam { get; set; } = null!;
    public MarkDistribution MarkDistribution { get; set; } = null!;
}
