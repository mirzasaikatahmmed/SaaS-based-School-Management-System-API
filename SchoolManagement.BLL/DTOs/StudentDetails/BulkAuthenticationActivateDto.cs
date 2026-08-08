namespace SchoolManagement.BLL.DTOs.StudentDetails;

public class BulkAuthenticationActivateDto
{
    public List<Guid> StudentIds { get; set; } = new();
}

public class BulkAuthenticationActivateResultDto
{
    public int Activated { get; set; }
    public int Failed { get; set; }
}
