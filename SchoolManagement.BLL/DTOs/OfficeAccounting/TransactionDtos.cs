namespace SchoolManagement.BLL.DTOs.OfficeAccounting;

public class TransactionFilterDto
{
    public Guid? AccountId { get; set; }
    public string? Type { get; set; }
    public Guid? VoucherHeadId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public class TransactionListItemDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Branch { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string VoucherHead { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? RefNo { get; set; }
    public string? PayVia { get; set; }
    public string? Description { get; set; }
    public decimal? Dr { get; set; }
    public decimal? Cr { get; set; }
    public decimal? RunningBalance { get; set; }
}

public class TransactionListResponseDto
{
    public IReadOnlyList<TransactionListItemDto> Data { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public decimal TotalDeposits { get; set; }
    public decimal TotalExpenses { get; set; }
}
