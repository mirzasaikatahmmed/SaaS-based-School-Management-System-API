namespace SchoolManagement.DAL.Entities.Tenant;

public class SalaryDeduction
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public SalaryTemplate Template { get; set; } = null!;
}
