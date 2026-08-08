namespace SchoolManagement.BLL.DTOs.StudentList;

public class BulkDeleteDto
{
    public List<Guid> StudentIds { get; set; } = new();
}

public class BulkDeleteResultDto
{
    public int Deleted { get; set; }
    public int Failed { get; set; }
}
