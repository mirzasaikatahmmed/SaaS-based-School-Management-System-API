namespace SchoolManagement.BLL.DTOs.OfficeAccounting;

public class CreateAccountingExpenseDto
{
    public Guid AccountId { get; set; }
    public Guid VoucherHeadId { get; set; }
    public string? RefNo { get; set; }
    public decimal Amount { get; set; }
    public DateTime? ExpenseDate { get; set; }
    public string? PayVia { get; set; }
    public string? Description { get; set; }
}

public class UpdateAccountingExpenseDto
{
    public Guid AccountId { get; set; }
    public Guid VoucherHeadId { get; set; }
    public string? RefNo { get; set; }
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string? PayVia { get; set; }
    public string? Description { get; set; }
}

public class AccountingExpenseFilterDto
{
    public Guid? AccountId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public class AccountingExpenseResponseDto
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public Guid VoucherHeadId { get; set; }
    public string VoucherHeadName { get; set; } = string.Empty;
    public string? RefNo { get; set; }
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string? PayVia { get; set; }
    public string? Description { get; set; }
    public string? AttachmentUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AccountingExpenseListResponseDto
{
    public IReadOnlyList<AccountingExpenseResponseDto> Data { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
