using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface ISalaryPaymentRepository
{
    Task<SalaryPayment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SalaryPayment?> GetByEmployeeAndMonthAsync(Guid employeeId, string paymentMonth, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SalaryPayment>> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default);
    Task<SalaryPayment> AddAsync(SalaryPayment entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(SalaryPayment entity, CancellationToken cancellationToken = default);
}
