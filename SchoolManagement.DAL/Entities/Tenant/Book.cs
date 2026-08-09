namespace SchoolManagement.DAL.Entities.Tenant;

public class Book
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? IsbnNo { get; set; }
    public string? Author { get; set; }
    public string? Edition { get; set; }
    public string? Publisher { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public Guid? CategoryId { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public string? CoverImageUrl { get; set; }
    public int TotalStock { get; set; }
    public int IssuedCopies { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public BookCategory? Category { get; set; }
    public ICollection<BookIssue> Issues { get; set; } = new List<BookIssue>();
}
