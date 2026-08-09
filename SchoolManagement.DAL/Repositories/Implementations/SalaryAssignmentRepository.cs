using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class SalaryAssignmentRepository(TenantDbContext context) : ISalaryAssignmentRepository
{
    public async Task<EmployeeSalaryAssignment?> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default)
        => await context.EmployeeSalaryAssignments
            .Include(a => a.Template)
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId, cancellationToken);

    public async Task<IReadOnlyList<EmployeeSalaryAssignment>> GetByEmployeeIdsAsync(IEnumerable<Guid> employeeIds, CancellationToken cancellationToken = default)
    {
        var ids = employeeIds.Distinct().ToList();
        return await context.EmployeeSalaryAssignments
            .Include(a => a.Template)
            .Where(a => ids.Contains(a.EmployeeId))
            .ToListAsync(cancellationToken);
    }

    public async Task<EmployeeSalaryAssignment> AddAsync(EmployeeSalaryAssignment entity, CancellationToken cancellationToken = default)
    {
        await context.EmployeeSalaryAssignments.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(EmployeeSalaryAssignment entity, CancellationToken cancellationToken = default)
    {
        context.EmployeeSalaryAssignments.Update(entity);
        return Task.CompletedTask;
    }
}
