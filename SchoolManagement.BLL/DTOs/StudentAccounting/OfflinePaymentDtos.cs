namespace SchoolManagement.BLL.DTOs.StudentAccounting;

public class CreateOfflinePaymentDto
{
    public Guid? StudentId { get; set; }
    public Guid? PaymentTypeId { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
}

public class OfflinePaymentFilterDto
{
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public class ReviewOfflinePaymentDto
{
    public string? Remarks { get; set; }
}

public class OfflinePaymentResponseDto
{
    public Guid Id { get; set; }
    public string TrxId { get; set; } = string.Empty;
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string RegisterNo { get; set; } = string.Empty;
    public Guid? PaymentTypeId { get; set; }
    public string? PaymentTypeName { get; set; }
    public string? ClassName { get; set; }
    public string? SectionName { get; set; }
    public DateTime PaymentDate { get; set; }
    public DateTime SubmitDate { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class OfflinePaymentListResponseDto
{
    public IReadOnlyList<OfflinePaymentResponseDto> Data { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class CreateOfflinePaymentTypeDto
{
    public string Name { get; set; } = string.Empty;
    public string? Instructions { get; set; }
}

public class UpdateOfflinePaymentTypeDto
{
    public string Name { get; set; } = string.Empty;
    public string? Instructions { get; set; }
    public bool? IsActive { get; set; }
}

public class OfflinePaymentTypeResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Instructions { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
