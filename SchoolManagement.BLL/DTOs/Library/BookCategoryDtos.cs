namespace SchoolManagement.BLL.DTOs.Library;

public class CreateBookCategoryDto
{
    public string Name { get; set; } = string.Empty;
}

public class UpdateBookCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public bool? IsActive { get; set; }
}

public class BookCategoryDto
{
    public Guid Id { get; set; }
    public string Branch { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int BookCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
