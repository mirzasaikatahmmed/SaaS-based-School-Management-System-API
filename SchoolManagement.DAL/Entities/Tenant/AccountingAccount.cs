namespace SchoolManagement.DAL.Entities.Tenant;

public class AccountingAccount
{
    public Guid Id { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string? AccountNumber { get; set; }
    public string? Description { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal CurrentBalance { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow.Date;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<AccountingDeposit> Deposits { get; set; } = new List<AccountingDeposit>();
    public ICollection<AccountingExpense> Expenses { get; set; } = new List<AccountingExpense>();
}
