namespace SchoolManagement.BLL.DTOs.OfficeAccounting;

public class CreateAccountingDepositDto
{
    public Guid AccountId { get; set; }
    public Guid VoucherHeadId { get; set; }
    public string? RefNo { get; set; }
    public decimal Amount { get; set; }
    public DateTime? DepositDate { get; set; }
    public string? PayVia { get; set; }
    public string? Description { get; set; }
}

public class UpdateAccountingDepositDto
{
    public Guid AccountId { get; set; }
    public Guid VoucherHeadId { get; set; }
    public string? RefNo { get; set; }
    public decimal Amount { get; set; }
    public DateTime DepositDate { get; set; }
    public string? PayVia { get; set; }
    public string? Description { get; set; }
}

public class AccountingDepositFilterDto
{
    public Guid? AccountId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public class AccountingDepositResponseDto
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public Guid VoucherHeadId { get; set; }
    public string VoucherHeadName { get; set; } = string.Empty;
    public string? RefNo { get; set; }
    public decimal Amount { get; set; }
    public DateTime DepositDate { get; set; }
    public string? PayVia { get; set; }
    public string? Description { get; set; }
    public string? AttachmentUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AccountingDepositListResponseDto
{
    public IReadOnlyList<AccountingDepositResponseDto> Data { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
