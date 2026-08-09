using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public class EmployeeSearchFilter
{
    public string? Role { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? DesignationId { get; set; }
    public string? Search { get; set; }
    public bool? IsActive { get; set; } = true;
    public bool? IsLoginActive { get; set; }
    public string? SortBy { get; set; }
    public string? SortDir { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Employee?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Employee?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Employee> Items, int TotalCount)> SearchAsync(EmployeeSearchFilter filter, CancellationToken cancellationToken = default);
    Task<bool> StaffIdExistsAsync(string staffId, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<Employee> AddAsync(Employee employee, CancellationToken cancellationToken = default);
    Task UpdateAsync(Employee employee, CancellationToken cancellationToken = default);

    Task<EmployeeImportBatch> AddImportBatchAsync(EmployeeImportBatch batch, CancellationToken cancellationToken = default);
    Task UpdateImportBatchAsync(EmployeeImportBatch batch, CancellationToken cancellationToken = default);
    Task AddImportBatchRowAsync(EmployeeImportBatchRow row, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmployeeImportBatch>> GetImportBatchesAsync(CancellationToken cancellationToken = default);
    Task<EmployeeImportBatch?> GetImportBatchByIdAsync(Guid batchId, CancellationToken cancellationToken = default);
}
