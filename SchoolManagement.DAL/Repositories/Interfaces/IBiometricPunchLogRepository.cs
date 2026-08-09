using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public class BiometricPunchLogFilter
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public Guid? DeviceId { get; set; }
    public string? Kind { get; set; }
    public Guid? StudentId { get; set; }
    public Guid? EmployeeId { get; set; }
    public string? DevicePin { get; set; }
    public string? Search { get; set; }
    /// <summary>Student | Teacher | Accountant | … — filters person type / employee role.</summary>
    public string? Role { get; set; }
    public Guid? ClassId { get; set; }
    public Guid? SectionId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public interface IBiometricPunchLogRepository
{
    Task<(IReadOnlyList<BiometricPunchLog> Items, int TotalCount)> GetFilteredAsync(
        DateTime? from, DateTime? to, Guid? deviceId, string? kind,
        int page, int pageSize, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<BiometricPunchLog> Items, int TotalCount)> GetFilteredAsync(
        BiometricPunchLogFilter filter, CancellationToken cancellationToken = default);

    Task<BiometricPunchLog> AddAsync(BiometricPunchLog entity, CancellationToken cancellationToken = default);
}
