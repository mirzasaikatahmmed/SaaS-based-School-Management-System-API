using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.Payroll;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class SalaryAssignService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http) : ISalaryAssignService
{
    public async Task<SalaryAssignListResponseDto> GetListAsync(SalaryAssignFilterDto filter, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        if (string.IsNullOrWhiteSpace(filter.Role))
            throw new AppException("Role is required.", 400);

        var page = filter.Page < 1 ? 1 : filter.Page;
        var size = filter.PageSize is < 1 or > 200 ? 25 : filter.PageSize;
        var (employees, total) = await uow.Employees.SearchAsync(new EmployeeSearchFilter
        {
            Role = filter.Role,
            DesignationId = filter.DesignationId,
            Search = filter.Search,
            IsActive = true,
            Page = page,
            PageSize = size
        }, ct);

        var assignments = await uow.SalaryAssignments.GetByEmployeeIdsAsync(employees.Select(e => e.Id), ct);
        var byEmployee = assignments.ToDictionary(a => a.EmployeeId);

        var data = employees.Select((e, i) =>
        {
            byEmployee.TryGetValue(e.Id, out var a);
            return new SalaryAssignItemDto
            {
                EmployeeId = e.Id,
                Sl = (page - 1) * size + i + 1,
                StaffId = e.StaffId,
                Name = e.Name,
                Designation = e.Designation?.Name,
                Department = e.Department?.Name,
                AssignedTemplateId = a?.TemplateId,
                AssignedSalaryGrade = a?.Template?.SalaryGrade,
                BasicSalary = a?.Template?.BasicSalary
            };
        }).ToList();

        return new SalaryAssignListResponseDto
        {
            Data = data,
            TotalCount = total,
            Page = page,
            PageSize = size,
            TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)size)
        };
    }

    public async Task<SalaryAssignItemDto> AssignAsync(Guid employeeId, AssignSalaryGradeDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var employee = await uow.Employees.GetByIdWithDetailsAsync(employeeId, ct)
            ?? throw new NotFoundException($"Employee '{employeeId}' not found.");
        var template = await uow.SalaryTemplates.GetByIdAsync(dto.TemplateId, ct)
            ?? throw new NotFoundException($"Salary template '{dto.TemplateId}' not found.");
        if (!template.IsActive)
            throw new AppException("Cannot assign an inactive salary template.", 400);

        var existing = await uow.SalaryAssignments.GetByEmployeeIdAsync(employeeId, ct);
        if (existing is null)
        {
            existing = new EmployeeSalaryAssignment
            {
                Id = Guid.NewGuid(),
                EmployeeId = employeeId,
                TemplateId = template.Id,
                AssignedAt = DateTime.UtcNow,
                AssignedBy = CurrentUserOrNull(),
                IsActive = true
            };
            await uow.SalaryAssignments.AddAsync(existing, ct);
        }
        else
        {
            existing.TemplateId = template.Id;
            existing.AssignedAt = DateTime.UtcNow;
            existing.AssignedBy = CurrentUserOrNull();
            existing.IsActive = true;
            await uow.SalaryAssignments.UpdateAsync(existing, ct);
        }

        await uow.SaveTenantChangesAsync(ct);

        return new SalaryAssignItemDto
        {
            EmployeeId = employee.Id,
            StaffId = employee.StaffId,
            Name = employee.Name,
            Designation = employee.Designation?.Name,
            Department = employee.Department?.Name,
            AssignedTemplateId = template.Id,
            AssignedSalaryGrade = template.SalaryGrade,
            BasicSalary = template.BasicSalary
        };
    }

    public async Task<BulkAssignSalaryResultDto> BulkAssignAsync(BulkAssignSalaryDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        if (dto.EmployeeIds.Count == 0)
            throw new AppException("EmployeeIds is required.", 400);

        var template = await uow.SalaryTemplates.GetByIdAsync(dto.TemplateId, ct)
            ?? throw new NotFoundException($"Salary template '{dto.TemplateId}' not found.");
        if (!template.IsActive)
            throw new AppException("Cannot assign an inactive salary template.", 400);

        var result = new BulkAssignSalaryResultDto();
        await uow.BeginTenantTransactionAsync(ct);
        try
        {
            foreach (var employeeId in dto.EmployeeIds.Distinct())
            {
                var employee = await uow.Employees.GetByIdAsync(employeeId, ct);
                if (employee is null || !employee.IsActive)
                {
                    result.Failed++;
                    continue;
                }

                var existing = await uow.SalaryAssignments.GetByEmployeeIdAsync(employeeId, ct);
                if (existing is null)
                {
                    await uow.SalaryAssignments.AddAsync(new EmployeeSalaryAssignment
                    {
                        Id = Guid.NewGuid(),
                        EmployeeId = employeeId,
                        TemplateId = template.Id,
                        AssignedAt = DateTime.UtcNow,
                        AssignedBy = CurrentUserOrNull(),
                        IsActive = true
                    }, ct);
                }
                else
                {
                    existing.TemplateId = template.Id;
                    existing.AssignedAt = DateTime.UtcNow;
                    existing.AssignedBy = CurrentUserOrNull();
                    existing.IsActive = true;
                    await uow.SalaryAssignments.UpdateAsync(existing, ct);
                }

                result.Assigned++;
            }

            await uow.SaveTenantChangesAsync(ct);
            await uow.CommitTenantTransactionAsync(ct);
            return result;
        }
        catch
        {
            await uow.RollbackTenantTransactionAsync(ct);
            throw;
        }
    }

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
            throw new ForbiddenException("Only Super Admin, School Admin, or Accountant can manage salary assignment.");
    }

    private Guid? CurrentUserOrNull()
    {
        var c = http.HttpContext?.User.FindFirst(AppConstants.Claims.UserId)
            ?? http.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        return c is not null && Guid.TryParse(c.Value, out var id) ? id : null;
    }
}
