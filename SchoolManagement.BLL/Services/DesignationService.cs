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
public class DesignationService(IUnitOfWork uow, ITenantContext tenant, ITenantSchemaProvisioner provisioner, IHttpContextAccessor http) : IDesignationService
{
    public async Task<IReadOnlyList<DesignationResponseDto>> GetAllAsync(CancellationToken ct = default) { await Ready(ct); Read(); return (await uow.Designations.GetAllAsync(ct)).Select(Map).ToList(); }
    public async Task<DesignationResponseDto> CreateAsync(CreateDesignationDto dto, CancellationToken ct = default) { await Ready(ct); Manage(); var name=dto.Name.Trim(); if(await uow.Designations.NameExistsAsync(name,null,ct)) throw new ConflictException($"Designation '{name}' already exists."); var x=new Designation {Id=Guid.NewGuid(),Name=name,IsActive=true,CreatedAt=DateTime.UtcNow,UpdatedAt=DateTime.UtcNow}; await uow.Designations.AddAsync(x,ct); await uow.SaveTenantChangesAsync(ct); return Map(x); }
    public async Task<DesignationResponseDto> UpdateAsync(Guid id, UpdateDesignationDto dto, CancellationToken ct = default) { await Ready(ct); Manage(); var x=await uow.Designations.GetByIdAsync(id,ct) ?? throw new NotFoundException($"Designation '{id}' not found."); var name=dto.Name.Trim(); if(await uow.Designations.NameExistsAsync(name,id,ct)) throw new ConflictException($"Designation '{name}' already exists."); x.Name=name; if(dto.IsActive.HasValue)x.IsActive=dto.IsActive.Value; x.UpdatedAt=DateTime.UtcNow; await uow.Designations.UpdateAsync(x,ct); await uow.SaveTenantChangesAsync(ct); return Map(x); }
    public async Task DeleteAsync(Guid id, CancellationToken ct = default) { await Ready(ct); Manage(); var x=await uow.Designations.GetByIdAsync(id,ct) ?? throw new NotFoundException($"Designation '{id}' not found."); var count=await uow.Designations.CountEmployeesUsingAsync(id,ct); if(count>0) throw new AppException($"Designation is in use by {count} employee(s) and cannot be deleted.",400); await uow.Designations.DeleteAsync(x,ct); await uow.SaveTenantChangesAsync(ct); }
    private DesignationResponseDto Map(Designation x)=>new(){Id=x.Id,Name=x.Name,IsActive=x.IsActive,CreatedAt=x.CreatedAt,Branch=tenant.TenantName??string.Empty};
    private async Task Ready(CancellationToken ct){if(string.IsNullOrEmpty(tenant.SchemaName))throw new AppException("X-Tenant-ID header is required.",400);await provisioner.EnsureEmployeeModuleAsync(tenant.SchemaName!,ct);}
    private HashSet<string> Roles(){var p=http.HttpContext?.User;if(p is null)return [];return p.FindAll("role").Concat(p.FindAll(ClaimTypes.Role)).Select(x=>x.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);}
    private void Manage(){var r=Roles();if(!r.Contains(AppConstants.Roles.Admin)&&!r.Contains(AppConstants.Roles.SuperAdmin))throw new ForbiddenException("Only Super Admin or School Admin can manage designations.");}
    private void Read(){var r=Roles();if(!r.Contains(AppConstants.Roles.Admin)&&!r.Contains(AppConstants.Roles.SuperAdmin)&&!r.Contains(AppConstants.Roles.Teacher))throw new ForbiddenException("You do not have access to designations.");}
}
