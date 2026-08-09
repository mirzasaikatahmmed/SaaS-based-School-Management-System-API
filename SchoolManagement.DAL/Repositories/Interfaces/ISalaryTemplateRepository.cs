using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface ISalaryTemplateRepository
{
    Task<IReadOnlyList<SalaryTemplate>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SalaryTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> GradeExistsAsync(string salaryGrade, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<int> CountAssignmentsAsync(Guid templateId, CancellationToken cancellationToken = default);
    Task<SalaryTemplate> AddAsync(SalaryTemplate entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(SalaryTemplate entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(SalaryTemplate entity, CancellationToken cancellationToken = default);
    Task ReplaceAllowancesAsync(Guid templateId, IEnumerable<SalaryAllowance> allowances, CancellationToken cancellationToken = default);
    Task ReplaceDeductionsAsync(Guid templateId, IEnumerable<SalaryDeduction> deductions, CancellationToken cancellationToken = default);
}
