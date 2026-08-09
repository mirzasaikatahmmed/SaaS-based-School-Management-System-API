using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.Employee;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;
public class DepartmentService(IUnitOfWork uow, ITenantContext tenant, ITenantSchemaProvisioner provisioner, IHttpContextAccessor http) : IDepartmentService
{
    public async Task<IReadOnlyList<DepartmentResponseDto>> GetAllAsync(CancellationToken ct = default) { await Ready(ct); Read(); return (await uow.Departments.GetAllAsync(ct)).Select(Map).ToList(); }
    public async Task<DepartmentResponseDto> CreateAsync(CreateDepartmentDto dto, CancellationToken ct = default) { await Ready(ct); Manage(); var name = dto.Name.Trim(); if (await uow.Departments.NameExistsAsync(name, null, ct)) throw new ConflictException($"Department '{name}' already exists."); var x = new Department { Id = Guid.NewGuid(), Name = name, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }; await uow.Departments.AddAsync(x, ct); await uow.SaveTenantChangesAsync(ct); return Map(x); }
    public async Task<DepartmentResponseDto> UpdateAsync(Guid id, UpdateDepartmentDto dto, CancellationToken ct = default) { await Ready(ct); Manage(); var x = await uow.Departments.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Department '{id}' not found."); var name = dto.Name.Trim(); if (await uow.Departments.NameExistsAsync(name, id, ct)) throw new ConflictException($"Department '{name}' already exists."); x.Name = name; if (dto.IsActive.HasValue) x.IsActive = dto.IsActive.Value; x.UpdatedAt = DateTime.UtcNow; await uow.Departments.UpdateAsync(x, ct); await uow.SaveTenantChangesAsync(ct); return Map(x); }
    public async Task DeleteAsync(Guid id, CancellationToken ct = default) { await Ready(ct); Manage(); var x = await uow.Departments.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Department '{id}' not found."); var count = await uow.Departments.CountEmployeesUsingAsync(id, ct); if (count > 0) throw new AppException($"Department is in use by {count} employee(s) and cannot be deleted.", 400); await uow.Departments.DeleteAsync(x, ct); await uow.SaveTenantChangesAsync(ct); }
    private DepartmentResponseDto Map(Department x) => new() { Id=x.Id, Name=x.Name, IsActive=x.IsActive, CreatedAt=x.CreatedAt, Branch=tenant.TenantName ?? string.Empty };
    private async Task Ready(CancellationToken ct) { if (string.IsNullOrEmpty(tenant.SchemaName)) throw new AppException("X-Tenant-ID header is required.",400); await provisioner.EnsureEmployeeModuleAsync(tenant.SchemaName!,ct); }
    private HashSet<string> Roles() => http.HttpContext?.User.FindAll("role").Concat(http.HttpContext.User.FindAll(ClaimTypes.Role)).Select(x=>x.Value).ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];
    private void Manage() { var r=Roles(); if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin)) throw new ForbiddenException("Only Super Admin or School Admin can manage departments."); }
    private void Read() { var r=Roles(); if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin) && !r.Contains(AppConstants.Roles.Teacher)) throw new ForbiddenException("You do not have access to departments."); }
}
