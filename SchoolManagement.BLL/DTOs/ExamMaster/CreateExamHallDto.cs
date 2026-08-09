namespace SchoolManagement.BLL.DTOs.ExamMaster;

public class CreateExamHallDto
{
    public string HallNo { get; set; } = string.Empty;
    public int NoOfSeats { get; set; }
}

public class UpdateExamHallDto
{
    public string HallNo { get; set; } = string.Empty;
    public int NoOfSeats { get; set; }
    public bool? IsActive { get; set; }
}

public class ExamHallResponseDto
{
    public Guid Id { get; set; }
    public int Sl { get; set; }
    public string Branch { get; set; } = string.Empty;
    public string HallNo { get; set; } = string.Empty;
    public int NoOfSeats { get; set; }
}

public class ExamHallLookupDto
{
    public Guid Id { get; set; }
    public string HallNo { get; set; } = string.Empty;
    public int NoOfSeats { get; set; }
}
