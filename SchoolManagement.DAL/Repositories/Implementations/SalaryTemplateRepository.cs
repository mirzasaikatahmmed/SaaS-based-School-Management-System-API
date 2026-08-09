using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class SalaryTemplateRepository(TenantDbContext context) : ISalaryTemplateRepository
{
    public async Task<IReadOnlyList<SalaryTemplate>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.SalaryTemplates
            .Include(t => t.Allowances.OrderBy(a => a.SortOrder))
            .Include(t => t.Deductions.OrderBy(d => d.SortOrder))
            .OrderBy(t => t.SalaryGrade)
            .ToListAsync(cancellationToken);

    public async Task<SalaryTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.SalaryTemplates
            .Include(t => t.Allowances.OrderBy(a => a.SortOrder))
            .Include(t => t.Deductions.OrderBy(d => d.SortOrder))
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<bool> GradeExistsAsync(string salaryGrade, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var name = salaryGrade.Trim().ToUpperInvariant();
        var q = context.SalaryTemplates.Where(t => t.SalaryGrade.ToUpper() == name);
        if (excludeId.HasValue) q = q.Where(t => t.Id != excludeId.Value);
        return await q.AnyAsync(cancellationToken);
    }

    public async Task<int> CountAssignmentsAsync(Guid templateId, CancellationToken cancellationToken = default)
        => await context.EmployeeSalaryAssignments.CountAsync(a => a.TemplateId == templateId && a.IsActive, cancellationToken);

    public async Task<SalaryTemplate> AddAsync(SalaryTemplate entity, CancellationToken cancellationToken = default)
    {
        await context.SalaryTemplates.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(SalaryTemplate entity, CancellationToken cancellationToken = default)
    {
        context.SalaryTemplates.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(SalaryTemplate entity, CancellationToken cancellationToken = default)
    {
        context.SalaryTemplates.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task ReplaceAllowancesAsync(Guid templateId, IEnumerable<SalaryAllowance> allowances, CancellationToken cancellationToken = default)
    {
        var existing = await context.SalaryAllowances.Where(a => a.TemplateId == templateId).ToListAsync(cancellationToken);
        context.SalaryAllowances.RemoveRange(existing);
        await context.SalaryAllowances.AddRangeAsync(allowances, cancellationToken);
    }

    public async Task ReplaceDeductionsAsync(Guid templateId, IEnumerable<SalaryDeduction> deductions, CancellationToken cancellationToken = default)
    {
        var existing = await context.SalaryDeductions.Where(d => d.TemplateId == templateId).ToListAsync(cancellationToken);
        context.SalaryDeductions.RemoveRange(existing);
        await context.SalaryDeductions.AddRangeAsync(deductions, cancellationToken);
    }
}
