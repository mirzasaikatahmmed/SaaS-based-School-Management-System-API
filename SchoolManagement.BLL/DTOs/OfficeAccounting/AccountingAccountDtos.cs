namespace SchoolManagement.BLL.DTOs.OfficeAccounting;

public class CreateAccountingAccountDto
{
    public string AccountName { get; set; } = string.Empty;
    public string? AccountNumber { get; set; }
    public string? Description { get; set; }
    public decimal OpeningBalance { get; set; }
    public DateTime? Date { get; set; }
}

public class UpdateAccountingAccountDto
{
    public string AccountName { get; set; } = string.Empty;
    public string? AccountNumber { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}

public class AccountingAccountResponseDto
{
    public Guid Id { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string? AccountNumber { get; set; }
    public string? Description { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal CurrentBalance { get; set; }
    public DateTime Date { get; set; }
    public bool IsActive { get; set; }
    public string Branch { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AccountingAccountLookupDto
{
    public Guid Id { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public decimal CurrentBalance { get; set; }
}
