namespace SchoolManagement.BLL.DTOs.OnlineAdmission;

public class UpdatePaymentStatusDto
{
    public string PaymentStatus { get; set; } = "Paid";
    public decimal? PaymentAmount { get; set; }
    public string? PaymentReference { get; set; }
    public DateTime? PaymentDate { get; set; }
}
