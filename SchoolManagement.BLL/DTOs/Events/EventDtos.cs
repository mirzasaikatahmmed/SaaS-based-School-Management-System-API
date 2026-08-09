namespace SchoolManagement.BLL.DTOs.Events;

public class CreateEventDto
{
    public string Title { get; set; } = string.Empty;
    public Guid? EventTypeId { get; set; }
    public bool IsHoliday { get; set; }
    public string Audience { get; set; } = "Everybody";
    public DateTime DateOfStart { get; set; }
    public DateTime DateOfEnd { get; set; }
    public string? Description { get; set; }
}

public class UpdateEventDto
{
    public string Title { get; set; } = string.Empty;
    public Guid? EventTypeId { get; set; }
    public bool IsHoliday { get; set; }
    public string Audience { get; set; } = "Everybody";
    public DateTime DateOfStart { get; set; }
    public DateTime DateOfEnd { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}

public class EventListItemDto
{
    public Guid Id { get; set; }
    public int Sl { get; set; }
    public string Branch { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? EventTypeName { get; set; }
    public bool IsHoliday { get; set; }
    public string Audience { get; set; } = string.Empty;
    public DateTime DateOfStart { get; set; }
    public DateTime DateOfEnd { get; set; }
    public string? ImageUrl { get; set; }
    public bool ShowWebsite { get; set; }
    public bool IsPublished { get; set; }
    public bool IsActive { get; set; }
    public string? CreatedByName { get; set; }
}

public class EventDetailDto : EventListItemDto
{
    public Guid? EventTypeId { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class EventFilterDto
{
    public string? Search { get; set; }
    public Guid? EventTypeId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public class EventListResponseDto
{
    public IReadOnlyList<EventListItemDto> Data { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class PublicEventDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? EventTypeName { get; set; }
    public bool IsHoliday { get; set; }
    public string Audience { get; set; } = string.Empty;
    public DateTime DateOfStart { get; set; }
    public DateTime DateOfEnd { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
}
