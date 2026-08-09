namespace SchoolManagement.BLL.DTOs.Reports;

public class LeaveReportFilterDto
{
    public string? Role { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string? Status { get; set; }
    public string? Search { get; set; }
    public string? Export { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 100;
}

public class LeaveReportRowDto
{
    public int Sl { get; set; }
    public Guid Id { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Applicant { get; set; } = string.Empty;
    public Guid EmployeeId { get; set; }
    public string LeaveCategory { get; set; } = string.Empty;
    public DateTime DateOfStart { get; set; }
    public DateTime DateOfEnd { get; set; }
    public int Days { get; set; }
    public DateTime ApplyDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class LeaveReportDto
{
    public string Title { get; set; } = "Leave List";
    public string? Role { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public IReadOnlyList<LeaveReportRowDto> Rows { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class PayrollSummaryFilterDto
{
    /// <summary>YYYY-MM</summary>
    public string Month { get; set; } = string.Empty;
    public string? Role { get; set; }
    public string? Search { get; set; }
    public string? Export { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 100;
}

public class PayrollSummaryRowDto
{
    public int Sl { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid? PaymentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public decimal Salary { get; set; }
    public decimal Allowance { get; set; }
    public decimal Deduction { get; set; }
    public decimal NetSalary { get; set; }
    public string? PayVia { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class PayrollSummaryReportDto
{
    public string Title { get; set; } = "Payroll Summary";
    public string Month { get; set; } = string.Empty;
    public string? Role { get; set; }
    public IReadOnlyList<PayrollSummaryRowDto> Rows { get; set; } = [];
    public decimal TotalSalary { get; set; }
    public decimal TotalAllowance { get; set; }
    public decimal TotalDeduction { get; set; }
    public decimal TotalNetSalary { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
