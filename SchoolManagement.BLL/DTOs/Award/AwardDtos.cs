namespace SchoolManagement.BLL.DTOs.Award;

public class GiveAwardDto
{
    public string Role { get; set; } = string.Empty;
    public Guid? EmployeeId { get; set; }
    public Guid? StudentId { get; set; }
    public string AwardName { get; set; } = string.Empty;
    public string GiftItem { get; set; } = string.Empty;
    public decimal? CashPrice { get; set; }
    public string AwardReason { get; set; } = string.Empty;
    public DateTime? GivenDate { get; set; }
}

public class UpdateAwardDto
{
    public string Role { get; set; } = string.Empty;
    public Guid? EmployeeId { get; set; }
    public Guid? StudentId { get; set; }
    public string AwardName { get; set; } = string.Empty;
    public string GiftItem { get; set; } = string.Empty;
    public decimal? CashPrice { get; set; }
    public string AwardReason { get; set; } = string.Empty;
    public DateTime GivenDate { get; set; }
}

public class AwardListItemDto
{
    public Guid Id { get; set; }
    public int Sl { get; set; }
    public string Branch { get; set; } = string.Empty;
    public string Winner { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string AwardName { get; set; } = string.Empty;
    public string GiftItem { get; set; } = string.Empty;
    public decimal? CashPrice { get; set; }
    public string AwardReason { get; set; } = string.Empty;
    public DateTime GivenDate { get; set; }
}

public class AwardResponseDto : AwardListItemDto
{
    public Guid? EmployeeId { get; set; }
    public Guid? StudentId { get; set; }
    public string RecipientType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AwardFilterDto
{
    public string? Role { get; set; }
    public string? Search { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string? Export { get; set; }
}

public class AwardListResponseDto
{
    public IReadOnlyList<AwardListItemDto> Data { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class WinnerLookupDto
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string RecipientType { get; set; } = string.Empty;
}
