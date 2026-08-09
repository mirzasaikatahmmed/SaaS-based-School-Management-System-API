using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public static class FeeInvoiceStatuses
{
    public const string Unpaid = "Unpaid";
    public const string Partial = "Partial";
    public const string Paid = "Paid";
}

public class StudentFeeInvoiceFilter
{
    public Guid? ClassId { get; set; }
    public Guid? SectionId { get; set; }
    public string? Status { get; set; }
    public Guid? StudentId { get; set; }
    public Guid? FeesAllocationId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public class DueInvoiceFilter
{
    public Guid? ClassId { get; set; }
    public Guid? SectionId { get; set; }
    public Guid? FeesTypeId { get; set; }
    public bool? OverdueOnly { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public interface IStudentFeeInvoiceRepository
{
    Task<(IReadOnlyList<StudentFeeInvoice> Items, int TotalCount)> GetFilteredAsync(StudentFeeInvoiceFilter filter, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<StudentFeeInvoice> Items, int TotalCount)> GetDueAsync(DueInvoiceFilter filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StudentFeeInvoice>> GetByStudentAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task<StudentFeeInvoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<StudentFeeInvoice?> GetByStudentAndAllocationAsync(Guid studentId, Guid allocationId, CancellationToken cancellationToken = default);
    Task<StudentFeeInvoice> AddAsync(StudentFeeInvoice entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(StudentFeeInvoice entity, CancellationToken cancellationToken = default);
}
