using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class SalaryPaymentRepository(TenantDbContext context) : ISalaryPaymentRepository
{
    public async Task<SalaryPayment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.SalaryPayments
            .Include(p => p.Employee).ThenInclude(e => e.Designation)
            .Include(p => p.Employee).ThenInclude(e => e.Department)
            .Include(p => p.Template)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<SalaryPayment?> GetByEmployeeAndMonthAsync(Guid employeeId, string paymentMonth, CancellationToken cancellationToken = default)
        => await context.SalaryPayments
            .Include(p => p.Template)
            .Include(p => p.Employee)
            .FirstOrDefaultAsync(p => p.EmployeeId == employeeId && p.PaymentMonth == paymentMonth, cancellationToken);

    public async Task<IReadOnlyList<SalaryPayment>> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default)
        => await context.SalaryPayments
            .Include(p => p.Template)
            .Where(p => p.EmployeeId == employeeId)
            .OrderByDescending(p => p.PaymentMonth)
            .ToListAsync(cancellationToken);

    public async Task<SalaryPayment> AddAsync(SalaryPayment entity, CancellationToken cancellationToken = default)
    {
        await context.SalaryPayments.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(SalaryPayment entity, CancellationToken cancellationToken = default)
    {
        context.SalaryPayments.Update(entity);
        return Task.CompletedTask;
    }
}
