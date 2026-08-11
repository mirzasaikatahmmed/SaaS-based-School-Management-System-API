using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public class StudentSearchFilter
{
    public string? Search { get; set; }
    public int? AcademicYear { get; set; }
    public Guid? ClassId { get; set; }
    public Guid? SectionId { get; set; }
    public Guid? CategoryId { get; set; }
    public bool? IsActive { get; set; } = true;
    public bool? IsLoginActive { get; set; }
    public string? SortBy { get; set; }
    public string? SortDir { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Student?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Student?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Student?> GetByRegisterNoAsync(string registerNo, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Student> Items, int TotalCount)> SearchAsync(StudentSearchFilter filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Student>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<Student> AddAsync(Student student, CancellationToken cancellationToken = default);
    Task UpdateAsync(Student student, CancellationToken cancellationToken = default);
    Task<bool> RegisterNoExistsAsync(string registerNo, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> SscRollExistsAsync(string sscRoll, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> RollExistsAsync(string roll, Guid classId, Guid sectionId, int academicYear, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<int> CountByAcademicYearAsync(int academicYear, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Student>> GetByGuardianUserIdAsync(Guid guardianUserId, CancellationToken cancellationToken = default);
}
