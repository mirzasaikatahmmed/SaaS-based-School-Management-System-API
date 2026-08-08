namespace SchoolManagement.BLL.DTOs.OnlineAdmission;

public class OnlineAdmissionFilterDto
{
    public Guid? ClassId { get; set; }
    public string? Status { get; set; }
    public string? PaymentStatus { get; set; }
    public int? AcademicYear { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Export { get; set; }
}
