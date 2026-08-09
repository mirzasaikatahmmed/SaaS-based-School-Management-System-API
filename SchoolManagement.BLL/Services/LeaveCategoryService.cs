using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.Leave;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class LeaveCategoryService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http) : ILeaveCategoryService
{
    public async Task<IReadOnlyList<LeaveCategoryResponseDto>> GetAllAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var items = await uow.LeaveCategories.GetAllAsync(ct);
        return items.Select((c, i) => Map(c, i + 1)).ToList();
    }

    public async Task<IReadOnlyList<LeaveCategoryLookupDto>> GetLookupAsync(string? role, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        IReadOnlyList<LeaveCategory> items = string.IsNullOrWhiteSpace(role)
            ? (await uow.LeaveCategories.GetAllAsync(ct)).Where(c => c.IsActive).ToList()
            : await uow.LeaveCategories.GetByRoleAsync(role, ct);
        return items.Select(c => new LeaveCategoryLookupDto
        {
            Id = c.Id,
            Name = c.Name,
            Days = c.Days,
            Role = c.Role
        }).ToList();
    }

    public async Task<LeaveCategoryResponseDto> CreateAsync(CreateLeaveCategoryDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var name = dto.Name.Trim();
        var role = CanonicalRole(dto.Role);
        if (await uow.LeaveCategories.NameRoleExistsAsync(name, role, null, ct))
            throw new ConflictException($"Leave category '{name}' already exists for role '{role}'.");
        var entity = new LeaveCategory
        {
            Id = Guid.NewGuid(),
            Name = name,
            Role = role,
            Days = dto.Days,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await uow.LeaveCategories.AddAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
        return Map(entity, 0);
    }

    public async Task<LeaveCategoryResponseDto> UpdateAsync(Guid id, UpdateLeaveCategoryDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var entity = await uow.LeaveCategories.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Leave category '{id}' not found.");
        var name = dto.Name.Trim();
        var role = CanonicalRole(dto.Role);
        if (await uow.LeaveCategories.NameRoleExistsAsync(name, role, id, ct))
            throw new ConflictException($"Leave category '{name}' already exists for role '{role}'.");
        entity.Name = name;
        entity.Role = role;
        entity.Days = dto.Days;
        if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;
        entity.UpdatedAt = DateTime.UtcNow;
        await uow.LeaveCategories.UpdateAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
        return Map(entity, 0);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var entity = await uow.LeaveCategories.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Leave category '{id}' not found.");
        var count = await uow.LeaveCategories.CountRequestsAsync(id, ct);
        if (count > 0)
            throw new AppException($"Category is in use by {count} leave request(s)", 400);
        await uow.LeaveCategories.DeleteAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
    }

    private LeaveCategoryResponseDto Map(LeaveCategory c, int sl) => new()
    {
        Id = c.Id,
        Sl = sl,
        Branch = tenant.TenantName ?? string.Empty,
        Name = c.Name,
        Role = c.Role,
        Days = c.Days,
        IsActive = c.IsActive,
        CreatedAt = c.CreatedAt
    };

    private static string CanonicalRole(string role)
        => EmployeeRoles.All.FirstOrDefault(x => x.Equals(role.Trim(), StringComparison.OrdinalIgnoreCase))
           ?? throw new AppException("Invalid employee role.", 400);

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
        return p.FindAll("role").Concat(p.FindAll(ClaimTypes.Role)).Select(x => x.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private void Manage()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin))
            throw new ForbiddenException("Only Super Admin or School Admin can manage leave categories.");
    }
}
