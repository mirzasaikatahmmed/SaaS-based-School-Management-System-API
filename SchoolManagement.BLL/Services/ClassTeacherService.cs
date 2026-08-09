using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.Academic;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class ClassTeacherService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http) : IClassTeacherService
{
    public async Task<IReadOnlyList<ClassTeacherResponseDto>> GetAllAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        return (await uow.ClassTeachers.GetAllAsync(ct)).Select(Map).ToList();
    }

    public async Task<ClassTeacherResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var x = await uow.ClassTeachers.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Class teacher allocation '{id}' not found.");
        return Map(x);
    }

    public async Task<ClassTeacherResponseDto> UpsertAsync(UpsertClassTeacherDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();

        var employee = await uow.Employees.GetByIdAsync(dto.EmployeeId, ct)
            ?? throw new NotFoundException("Employee not found.");
        if (!employee.Role.Equals(EmployeeRoles.Teacher, StringComparison.OrdinalIgnoreCase))
            throw new AppException("Only employees with the Teacher role can be assigned as a class teacher.", 400);

        var existing = await uow.ClassTeachers.GetByClassSectionAsync(dto.ClassId, dto.SectionId, ct);
        if (existing is not null)
        {
            existing.EmployeeId = dto.EmployeeId;
            existing.IsActive = true;
            existing.UpdatedAt = DateTime.UtcNow;
            await uow.ClassTeachers.UpdateAsync(existing, ct);
            await uow.SaveTenantChangesAsync(ct);
            return Map(await uow.ClassTeachers.GetByIdAsync(existing.Id, ct) ?? existing);
        }

        var entity = new ClassTeacherAllocation
        {
            Id = Guid.NewGuid(),
            ClassId = dto.ClassId,
            SectionId = dto.SectionId,
            EmployeeId = dto.EmployeeId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await uow.ClassTeachers.AddAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
        return Map(await uow.ClassTeachers.GetByIdAsync(entity.Id, ct) ?? entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var x = await uow.ClassTeachers.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Class teacher allocation '{id}' not found.");
        await uow.ClassTeachers.DeleteAsync(x, ct);
        await uow.SaveTenantChangesAsync(ct);
    }

    private static ClassTeacherResponseDto Map(ClassTeacherAllocation x) => new()
    {
        Id = x.Id,
        ClassId = x.ClassId,
        ClassName = x.Class?.Name ?? string.Empty,
        SectionId = x.SectionId,
        SectionName = x.Section?.Name ?? string.Empty,
        EmployeeId = x.EmployeeId,
        EmployeeName = x.Employee?.Name ?? string.Empty,
        IsActive = x.IsActive,
        CreatedAt = x.CreatedAt
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
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin))
            throw new ForbiddenException("Only Super Admin or School Admin can manage class teachers.");
    }
}
