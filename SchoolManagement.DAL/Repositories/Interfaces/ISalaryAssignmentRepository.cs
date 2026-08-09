using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface ISalaryAssignmentRepository
{
    Task<EmployeeSalaryAssignment?> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmployeeSalaryAssignment>> GetByEmployeeIdsAsync(IEnumerable<Guid> employeeIds, CancellationToken cancellationToken = default);
    Task<EmployeeSalaryAssignment> AddAsync(EmployeeSalaryAssignment entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(EmployeeSalaryAssignment entity, CancellationToken cancellationToken = default);
}
