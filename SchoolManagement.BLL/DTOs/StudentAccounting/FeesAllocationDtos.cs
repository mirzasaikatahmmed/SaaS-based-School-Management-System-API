namespace SchoolManagement.BLL.DTOs.StudentAccounting;

public class CreateFeesAllocationDto
{
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public Guid FeesGroupId { get; set; }
    public int AcademicYear { get; set; }
}

public class UpdateFeesAllocationDto
{
    public bool? IsActive { get; set; }
}

public class FeesAllocationFilterDto
{
    public Guid? ClassId { get; set; }
    public Guid? SectionId { get; set; }
    public int? AcademicYear { get; set; }
    public bool? IsActive { get; set; }
}

public class FeesAllocationResponseDto
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public Guid SectionId { get; set; }
    public string SectionName { get; set; } = string.Empty;
    public Guid FeesGroupId { get; set; }
    public string FeesGroupName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int AcademicYear { get; set; }
    public bool IsActive { get; set; }
    public int InvoicesGenerated { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GenerateInvoicesDto
{
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public Guid? FeesAllocationId { get; set; }
}

public class GenerateInvoicesResultDto
{
    public int Generated { get; set; }
    public int Skipped { get; set; }
    public int TotalStudents { get; set; }
}
