namespace SchoolManagement.BLL.DTOs.Library;

public class CreateBookDto
{
    public string Title { get; set; } = string.Empty;
    public string? IsbnNo { get; set; }
    public string? Author { get; set; }
    public string? Edition { get; set; }
    public string? Publisher { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public Guid? CategoryId { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public int TotalStock { get; set; }
}

public class UpdateBookDto
{
    public string Title { get; set; } = string.Empty;
    public string? IsbnNo { get; set; }
    public string? Author { get; set; }
    public string? Edition { get; set; }
    public string? Publisher { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public Guid? CategoryId { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public int TotalStock { get; set; }
    public bool? IsActive { get; set; }
}

public class BookListItemDto
{
    public Guid Id { get; set; }
    public int Sl { get; set; }
    public string Branch { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? IsbnNo { get; set; }
    public string? Author { get; set; }
    public string? CategoryName { get; set; }
    public string? CoverImageUrl { get; set; }
    public int TotalStock { get; set; }
    public int IssuedCopies { get; set; }
    public int AvailableCopies { get; set; }
    public bool IsActive { get; set; }
}

public class BookDetailDto : BookListItemDto
{
    public Guid? CategoryId { get; set; }
    public string? Edition { get; set; }
    public string? Publisher { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class BookFilterDto
{
    public Guid? CategoryId { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public class BookListResponseDto
{
    public IReadOnlyList<BookListItemDto> Data { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class BookLookupDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int AvailableCopies { get; set; }
}
