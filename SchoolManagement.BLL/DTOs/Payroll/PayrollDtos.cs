namespace SchoolManagement.BLL.DTOs.Payroll;

public class AllowanceRowDto
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int SortOrder { get; set; }
}

public class DeductionRowDto
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int SortOrder { get; set; }
}

public class CreateSalaryTemplateDto
{
    public string SalaryGrade { get; set; } = string.Empty;
    public decimal BasicSalary { get; set; }
    public decimal? OvertimeRatePerHour { get; set; }
    public List<AllowanceRowDto> Allowances { get; set; } = [];
    public List<DeductionRowDto> Deductions { get; set; } = [];
}

public class UpdateSalaryTemplateDto : CreateSalaryTemplateDto
{
    public bool? IsActive { get; set; }
}

public class SalaryTemplateListItemDto
{
    public Guid Id { get; set; }
    public int Sl { get; set; }
    public string SalaryGrade { get; set; } = string.Empty;
    public decimal BasicSalary { get; set; }
    public decimal TotalAllowance { get; set; }
    public decimal TotalDeduction { get; set; }
    public decimal NetSalary { get; set; }
    public int AssignedEmployeeCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SalaryTemplateResponseDto
{
    public Guid Id { get; set; }
    public int Sl { get; set; }
    public string SalaryGrade { get; set; } = string.Empty;
    public decimal BasicSalary { get; set; }
    public decimal? OvertimeRatePerHour { get; set; }
    public List<AllowanceRowDto> Allowances { get; set; } = [];
    public List<DeductionRowDto> Deductions { get; set; } = [];
    public decimal TotalAllowance { get; set; }
    public decimal TotalDeduction { get; set; }
    public decimal NetSalary { get; set; }
    public int AssignedEmployeeCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SalaryTemplateLookupDto
{
    public Guid Id { get; set; }
    public string SalaryGrade { get; set; } = string.Empty;
    public decimal BasicSalary { get; set; }
    public decimal NetSalary { get; set; }
}

public class SalaryAssignFilterDto
{
    public string? Role { get; set; }
    public Guid? DesignationId { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public class SalaryAssignItemDto
{
    public Guid EmployeeId { get; set; }
    public int Sl { get; set; }
    public string StaffId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public string? Department { get; set; }
    public Guid? AssignedTemplateId { get; set; }
    public string? AssignedSalaryGrade { get; set; }
    public decimal? BasicSalary { get; set; }
}

public class SalaryAssignListResponseDto
{
    public IReadOnlyList<SalaryAssignItemDto> Data { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class AssignSalaryGradeDto
{
    public Guid TemplateId { get; set; }
}

public class BulkAssignSalaryDto
{
    public List<Guid> EmployeeIds { get; set; } = [];
    public Guid TemplateId { get; set; }
}

public class BulkAssignSalaryResultDto
{
    public int Assigned { get; set; }
    public int Failed { get; set; }
}

public class SalaryPaymentFilterDto
{
    public string? Role { get; set; }
    public string PaymentMonth { get; set; } = string.Empty;
    public string? Search { get; set; }
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string? Export { get; set; }
}

public class SalaryPaymentItemDto
{
    public Guid EmployeeId { get; set; }
    public Guid? PaymentId { get; set; }
    public string StaffId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public string? Department { get; set; }
    public string? MobileNo { get; set; }
    public string? SalaryGrade { get; set; }
    public decimal? BasicSalary { get; set; }
    public string Status { get; set; } = "Unpaid";
    public DateTime? PaymentDate { get; set; }
}

public class SalaryPaymentListResponseDto
{
    public IReadOnlyList<SalaryPaymentItemDto> Data { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ProcessPaymentDto
{
    public string PaymentMonth { get; set; } = string.Empty;
    public decimal OvertimeHours { get; set; }
    public decimal AdvanceDeduction { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PaymentNote { get; set; }
}

public class SalaryPaymentResponseDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string StaffId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public Guid TemplateId { get; set; }
    public string SalaryGrade { get; set; } = string.Empty;
    public string PaymentMonth { get; set; } = string.Empty;
    public decimal BasicSalary { get; set; }
    public decimal TotalAllowance { get; set; }
    public decimal TotalDeduction { get; set; }
    public decimal NetSalary { get; set; }
    public decimal OvertimeHours { get; set; }
    public decimal OvertimeAmount { get; set; }
    public decimal AdvanceDeduction { get; set; }
    public decimal FinalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? PaymentDate { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PaymentNote { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MySalaryDto
{
    public Guid? AssignmentId { get; set; }
    public Guid? TemplateId { get; set; }
    public string? SalaryGrade { get; set; }
    public decimal? BasicSalary { get; set; }
    public decimal? TotalAllowance { get; set; }
    public decimal? TotalDeduction { get; set; }
    public decimal? NetSalary { get; set; }
    public decimal? OvertimeRatePerHour { get; set; }
    public List<AllowanceRowDto> Allowances { get; set; } = [];
    public List<DeductionRowDto> Deductions { get; set; } = [];
}
