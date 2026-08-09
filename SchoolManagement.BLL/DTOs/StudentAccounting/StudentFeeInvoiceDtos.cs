namespace SchoolManagement.BLL.DTOs.StudentAccounting;

public class StudentFeeInvoiceFilterDto
{
    public Guid? ClassId { get; set; }
    public Guid? SectionId { get; set; }
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string? Export { get; set; }
}

public class DueInvoiceFilterDto
{
    public Guid? ClassId { get; set; }
    public Guid? SectionId { get; set; }
    public Guid? FeesTypeId { get; set; }
    public bool? OverdueOnly { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public class PayInvoiceDto
{
    public decimal Amount { get; set; }
}

public class StudentFeeInvoiceResponseDto
{
    public Guid Id { get; set; }
    public int Sl { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string RegisterNo { get; set; } = string.Empty;
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public Guid SectionId { get; set; }
    public string SectionName { get; set; } = string.Empty;
    public Guid FeesGroupId { get; set; }
    public string FeesGroupName { get; set; } = string.Empty;
    public Guid FeesAllocationId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal FineAmount { get; set; }
    public decimal DueAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Branch { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class StudentFeeInvoiceListResponseDto
{
    public IReadOnlyList<StudentFeeInvoiceResponseDto> Data { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
