namespace SchoolManagement.BLL.DTOs.Import;

public class ImportBatchResponseDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
    public string? ClassName { get; set; }
    public string? SectionName { get; set; }
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid ImportedBy { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<ImportRowResultDto>? Rows { get; set; }
}

public class ImportBatchListResponseDto
{
    public IReadOnlyList<ImportBatchResponseDto> Items { get; set; } = Array.Empty<ImportBatchResponseDto>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}

public class StudentImportRowDto
{
    public int RowNumber { get; set; }
    public Dictionary<string, string> RawData { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public string? RegisterNo { get; set; }
    public string? Roll { get; set; }
    public string? AcademicYear { get; set; }
    public string? AdmissionDate { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Gender { get; set; }
    public string? DateOfBirth { get; set; }
    public string? BloodGroup { get; set; }
    public string? Religion { get; set; }
    public string? Caste { get; set; }
    public string? MotherTongue { get; set; }
    public string? MobileNo { get; set; }
    public string? Email { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PresentAddress { get; set; }
    public string? PermanentAddress { get; set; }
    public string? FathersNidNumber { get; set; }
    public string? MothersNidNumber { get; set; }
    public string? BirthRegistrationNumber { get; set; }
    public string? CategoryId { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? GuardianName { get; set; }
    public string? GuardianRelation { get; set; }
    public string? GuardianMobile { get; set; }
    public string? GuardianEmail { get; set; }
    public string? FatherName { get; set; }
    public string? MotherName { get; set; }
    public string? GuardianOccupation { get; set; }
    public string? GuardianIncome { get; set; }
    public string? GuardianEducation { get; set; }
    public string? GuardianAddress { get; set; }
    public string? GuardianUsername { get; set; }
    public string? GuardianPassword { get; set; }
    public string? TransportRoute { get; set; }
    public string? HostelName { get; set; }
    public string? RoomName { get; set; }
    public string? PreviousSchoolName { get; set; }
    public string? PreviousSchoolQualification { get; set; }
    public string? Remarks { get; set; }
}
