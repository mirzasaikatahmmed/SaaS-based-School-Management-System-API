using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.Payroll;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class SalaryTemplateService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http) : ISalaryTemplateService
{
    public async Task<IReadOnlyList<SalaryTemplateListItemDto>> GetListAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var items = await uow.SalaryTemplates.GetAllAsync(ct);
        var result = new List<SalaryTemplateListItemDto>();
        for (var i = 0; i < items.Count; i++)
        {
            var t = items[i];
            result.Add(new SalaryTemplateListItemDto
            {
                Id = t.Id,
                Sl = i + 1,
                SalaryGrade = t.SalaryGrade,
                BasicSalary = t.BasicSalary,
                TotalAllowance = t.TotalAllowance,
                TotalDeduction = t.TotalDeduction,
                NetSalary = t.NetSalary,
                AssignedEmployeeCount = await uow.SalaryTemplates.CountAssignmentsAsync(t.Id, ct),
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt
            });
        }
        return result;
    }

    public async Task<SalaryTemplateResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var t = await uow.SalaryTemplates.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Salary template '{id}' not found.");
        return await Map(t, 0, ct);
    }

    public async Task<IReadOnlyList<SalaryTemplateLookupDto>> GetLookupAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        return (await uow.SalaryTemplates.GetAllAsync(ct))
            .Where(t => t.IsActive)
            .Select(t => new SalaryTemplateLookupDto
            {
                Id = t.Id,
                SalaryGrade = t.SalaryGrade,
                BasicSalary = t.BasicSalary,
                NetSalary = t.NetSalary
            })
            .ToList();
    }

    public async Task<SalaryTemplateResponseDto> CreateAsync(CreateSalaryTemplateDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var grade = dto.SalaryGrade.Trim();
        if (await uow.SalaryTemplates.GradeExistsAsync(grade, null, ct))
            throw new ConflictException($"Salary grade '{grade}' already exists.");

        var (totalAllowance, totalDeduction, net) = Compute(dto.BasicSalary, dto.Allowances, dto.Deductions);
        var template = new SalaryTemplate
        {
            Id = Guid.NewGuid(),
            SalaryGrade = grade,
            BasicSalary = dto.BasicSalary,
            OvertimeRatePerHour = dto.OvertimeRatePerHour,
            TotalAllowance = totalAllowance,
            TotalDeduction = totalDeduction,
            NetSalary = net,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        foreach (var (row, i) in dto.Allowances.Select((a, i) => (a, i)))
        {
            template.Allowances.Add(new SalaryAllowance
            {
                Id = Guid.NewGuid(),
                TemplateId = template.Id,
                Name = row.Name.Trim(),
                Amount = row.Amount,
                SortOrder = row.SortOrder != 0 ? row.SortOrder : i,
                CreatedAt = DateTime.UtcNow
            });
        }

        foreach (var (row, i) in dto.Deductions.Select((d, i) => (d, i)))
        {
            template.Deductions.Add(new SalaryDeduction
            {
                Id = Guid.NewGuid(),
                TemplateId = template.Id,
                Name = row.Name.Trim(),
                Amount = row.Amount,
                SortOrder = row.SortOrder != 0 ? row.SortOrder : i,
                CreatedAt = DateTime.UtcNow
            });
        }

        await uow.SalaryTemplates.AddAsync(template, ct);
        await uow.SaveTenantChangesAsync(ct);
        return await Map(await uow.SalaryTemplates.GetByIdAsync(template.Id, ct) ?? template, 0, ct);
    }

    public async Task<SalaryTemplateResponseDto> UpdateAsync(Guid id, UpdateSalaryTemplateDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var template = await uow.SalaryTemplates.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Salary template '{id}' not found.");

        var grade = dto.SalaryGrade.Trim();
        if (await uow.SalaryTemplates.GradeExistsAsync(grade, id, ct))
            throw new ConflictException($"Salary grade '{grade}' already exists.");

        var (totalAllowance, totalDeduction, net) = Compute(dto.BasicSalary, dto.Allowances, dto.Deductions);
        template.SalaryGrade = grade;
        template.BasicSalary = dto.BasicSalary;
        template.OvertimeRatePerHour = dto.OvertimeRatePerHour;
        template.TotalAllowance = totalAllowance;
        template.TotalDeduction = totalDeduction;
        template.NetSalary = net;
        if (dto.IsActive.HasValue) template.IsActive = dto.IsActive.Value;
        template.UpdatedAt = DateTime.UtcNow;

        var allowances = dto.Allowances.Select((row, i) => new SalaryAllowance
        {
            Id = Guid.NewGuid(),
            TemplateId = template.Id,
            Name = row.Name.Trim(),
            Amount = row.Amount,
            SortOrder = row.SortOrder != 0 ? row.SortOrder : i,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        var deductions = dto.Deductions.Select((row, i) => new SalaryDeduction
        {
            Id = Guid.NewGuid(),
            TemplateId = template.Id,
            Name = row.Name.Trim(),
            Amount = row.Amount,
            SortOrder = row.SortOrder != 0 ? row.SortOrder : i,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        await uow.SalaryTemplates.ReplaceAllowancesAsync(template.Id, allowances, ct);
        await uow.SalaryTemplates.ReplaceDeductionsAsync(template.Id, deductions, ct);
        await uow.SalaryTemplates.UpdateAsync(template, ct);
        await uow.SaveTenantChangesAsync(ct);
        return await Map(await uow.SalaryTemplates.GetByIdAsync(id, ct) ?? template, 0, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var template = await uow.SalaryTemplates.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Salary template '{id}' not found.");
        var count = await uow.SalaryTemplates.CountAssignmentsAsync(id, ct);
        if (count > 0)
            throw new AppException($"Template is assigned to {count} employee(s)", 400);
        await uow.SalaryTemplates.DeleteAsync(template, ct);
        await uow.SaveTenantChangesAsync(ct);
    }

    private static (decimal TotalAllowance, decimal TotalDeduction, decimal NetSalary) Compute(
        decimal basic,
        IEnumerable<AllowanceRowDto> allowances,
        IEnumerable<DeductionRowDto> deductions)
    {
        var totalAllowance = allowances.Sum(a => a.Amount);
        var totalDeduction = deductions.Sum(d => d.Amount);
        return (totalAllowance, totalDeduction, basic + totalAllowance - totalDeduction);
    }

    private async Task<SalaryTemplateResponseDto> Map(SalaryTemplate t, int sl, CancellationToken ct) => new()
    {
        Id = t.Id,
        Sl = sl,
        SalaryGrade = t.SalaryGrade,
        BasicSalary = t.BasicSalary,
        OvertimeRatePerHour = t.OvertimeRatePerHour,
        Allowances = t.Allowances.OrderBy(a => a.SortOrder).Select(a => new AllowanceRowDto
        {
            Id = a.Id, Name = a.Name, Amount = a.Amount, SortOrder = a.SortOrder
        }).ToList(),
        Deductions = t.Deductions.OrderBy(d => d.SortOrder).Select(d => new DeductionRowDto
        {
            Id = d.Id, Name = d.Name, Amount = d.Amount, SortOrder = d.SortOrder
        }).ToList(),
        TotalAllowance = t.TotalAllowance,
        TotalDeduction = t.TotalDeduction,
        NetSalary = t.NetSalary,
        AssignedEmployeeCount = await uow.SalaryTemplates.CountAssignmentsAsync(t.Id, ct),
        IsActive = t.IsActive,
        CreatedAt = t.CreatedAt
    };

    private async Task Ready(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(tenant.SchemaName))
            throw new AppException("X-Tenant-ID header is required.", 400);
        await provisioner.EnsureEmployeeModuleAsync(tenant.SchemaName!, ct);
    }

    private HashSet<string> Roles()
    {
        var p = http.HttpContext?.User;
        if (p is null) return [];
        return p.FindAll("role").Concat(p.FindAll(ClaimTypes.Role))
            .Select(x => x.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private void Manage()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) &&
            !r.Contains(AppConstants.Roles.SuperAdmin) &&
            !r.Contains(AppConstants.Roles.Accountant))
            throw new ForbiddenException("Only Super Admin, School Admin, or Accountant can manage payroll.");
    }
}
