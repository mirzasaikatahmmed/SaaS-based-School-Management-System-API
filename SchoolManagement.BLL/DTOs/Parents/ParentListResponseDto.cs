namespace SchoolManagement.BLL.DTOs.Parents;

public class ParentListResponseDto
{
    public List<ParentListItemDto> Data { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
