namespace SchoolManagement.DAL.Entities.Tenant;

public class SalaryTemplate
{
    public Guid Id { get; set; }
    public string SalaryGrade { get; set; } = string.Empty;
    public decimal BasicSalary { get; set; }
    public decimal? OvertimeRatePerHour { get; set; }
    public decimal TotalAllowance { get; set; }
    public decimal TotalDeduction { get; set; }
    public decimal NetSalary { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<SalaryAllowance> Allowances { get; set; } = new List<SalaryAllowance>();
    public ICollection<SalaryDeduction> Deductions { get; set; } = new List<SalaryDeduction>();
    public ICollection<EmployeeSalaryAssignment> Assignments { get; set; } = new List<EmployeeSalaryAssignment>();
}
