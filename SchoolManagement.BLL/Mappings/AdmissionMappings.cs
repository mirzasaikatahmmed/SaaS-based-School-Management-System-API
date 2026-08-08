using SchoolManagement.BLL.DTOs.OnlineAdmission;
using SchoolManagement.BLL.DTOs.Student;
using SchoolManagement.BLL.DTOs.StudentCategory;
using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.BLL.Mappings;

/// <summary>Manual entity → DTO mappings for the admission module.</summary>
public static class AdmissionMappings
{
    public static AdmissionLookupItemDto ToLookup(ClassEntity c) => new()
    {
        Id = c.Id, Name = c.Name, NumericName = c.NumericName
    };

    public static AdmissionLookupItemDto ToLookup(Section s) => new()
    {
        Id = s.Id, Name = s.Name, ParentId = s.ClassId
    };

    public static AdmissionLookupItemDto ToLookup(StudentCategory c) => new()
    {
        Id = c.Id, Name = c.Name
    };

    public static StudentCategoryResponseDto ToCategoryDto(StudentCategory c, int sl, string branch) => new()
    {
        Id = c.Id,
        Sl = sl,
        Branch = branch,
        Name = c.Name,
        IsActive = c.IsActive,
        CreatedAt = c.CreatedAt
    };

    public static AdmissionLookupItemDto ToLookup(TransportRoute r) => new()
    {
        Id = r.Id, Name = r.Name
    };

    public static AdmissionLookupItemDto ToLookup(Hostel h) => new()
    {
        Id = h.Id, Name = h.Name
    };

    public static AdmissionLookupItemDto ToLookup(HostelRoom r) => new()
    {
        Id = r.Id, Name = r.Name, ParentId = r.HostelId
    };

    public static GuardianDto ToDto(Guardian g, string? profilePictureUrl = null) => new()
    {
        Id = g.Id,
        Name = g.Name,
        Relation = g.Relation,
        FatherName = g.FatherName,
        MotherName = g.MotherName,
        Occupation = g.Occupation,
        Income = g.Income,
        Education = g.Education,
        City = g.City,
        State = g.State,
        MobileNo = g.MobileNo,
        Email = g.Email,
        Address = g.Address,
        ProfilePictureUrl = profilePictureUrl ?? g.ProfilePictureUrl,
        IsPrimary = g.IsPrimary
    };

    public static OnlineAdmissionResponseDto ToDto(OnlineAdmission o, int sl = 1)
    {
        var name = string.IsNullOrWhiteSpace(o.LastName) ? o.FirstName : $"{o.FirstName} {o.LastName}";
        return new OnlineAdmissionResponseDto
        {
            Id = o.Id,
            Sl = sl,
            ReferenceNo = o.ReferenceNo,
            Name = name,
            FirstName = o.FirstName,
            LastName = o.LastName,
            Gender = o.Gender,
            ClassId = o.ClassId,
            ClassName = o.ClassName ?? o.Class?.Name,
            MobileNo = o.MobileNo,
            Status = o.Status,
            PaymentStatus = o.PaymentStatus,
            ApplyDate = o.ApplyDate,
            DeclineReason = o.DeclineReason,
            StudentId = o.StudentId,
            AcademicYear = o.AcademicYear
        };
    }
}
