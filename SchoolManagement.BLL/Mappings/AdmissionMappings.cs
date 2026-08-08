using SchoolManagement.BLL.DTOs.Student;
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
}
