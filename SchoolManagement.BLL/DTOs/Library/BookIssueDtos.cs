namespace SchoolManagement.BLL.DTOs.Library;

public class IssueBookDto
{
    public Guid BookId { get; set; }
    public string Role { get; set; } = string.Empty;
    public Guid? StudentId { get; set; }
    public Guid? EmployeeId { get; set; }
    public DateTime? DateOfIssue { get; set; }
    public DateTime DateOfExpiry { get; set; }
}

public class ReturnBookDto
{
    public DateTime? ReturnDate { get; set; }
    public decimal? FineOverride { get; set; }
}

public class BookIssueListItemDto
{
    public Guid Id { get; set; }
    public int Sl { get; set; }
    public string Branch { get; set; } = string.Empty;
    public Guid BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string BorrowerName { get; set; } = string.Empty;
    public DateTime DateOfIssue { get; set; }
    public DateTime DateOfExpiry { get; set; }
    public DateTime? ReturnDate { get; set; }
    public decimal Fine { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class BookIssueFilterDto
{
    public string? Status { get; set; }
    public string? Role { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public class BookIssueListResponseDto
{
    public IReadOnlyList<BookIssueListItemDto> Data { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class BorrowerLookupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
