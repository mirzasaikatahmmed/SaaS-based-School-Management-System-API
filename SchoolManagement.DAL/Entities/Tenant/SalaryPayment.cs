namespace SchoolManagement.DAL.Entities.Tenant;

public class SalaryPayment
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid TemplateId { get; set; }
    public string PaymentMonth { get; set; } = string.Empty;
    public decimal BasicSalary { get; set; }
    public decimal TotalAllowance { get; set; }
    public decimal TotalDeduction { get; set; }
    public decimal NetSalary { get; set; }
    public decimal OvertimeHours { get; set; }
    public decimal OvertimeAmount { get; set; }
    public decimal AdvanceDeduction { get; set; }
    public decimal FinalAmount { get; set; }
    public string Status { get; set; } = SalaryPaymentStatuses.Unpaid;
    public DateTime? PaymentDate { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PaymentNote { get; set; }
    public Guid? PaidBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Employee Employee { get; set; } = null!;
    public SalaryTemplate Template { get; set; } = null!;
    public User? PaidByUser { get; set; }
}

public static class SalaryPaymentStatuses
{
    public const string Unpaid = "Unpaid";
    public const string Paid = "Paid";
    public const string NoGradeAssigned = "No Grade Assigned";
}
