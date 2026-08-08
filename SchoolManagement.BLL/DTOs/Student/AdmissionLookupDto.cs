namespace SchoolManagement.BLL.DTOs.Student;

public class AdmissionLookupItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? NumericName { get; set; }
    public Guid? ParentId { get; set; }
}

public class NextRegisterNoDto
{
    public string RegisterNo { get; set; } = string.Empty;
    public int AcademicYear { get; set; }
    public int Sequence { get; set; }
}
