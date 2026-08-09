using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.ExamMaster;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class MarkDistributionService(IUnitOfWork uow, ITenantContext tenant, ITenantSchemaProvisioner provisioner, IHttpContextAccessor http) : IMarkDistributionService
{
    public async Task<IReadOnlyList<MarkDistributionResponseDto>> GetAllAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        Read();
        var items = await uow.MarkDistributions.GetAllAsync(ct);
        return items.Select((x, i) => Map(x, i + 1)).ToList();
    }

    public async Task<MarkDistributionResponseDto> CreateAsync(CreateMarkDistributionDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var name = dto.Name.Trim();
        if (await uow.MarkDistributions.NameExistsAsync(name, null, ct))
            throw new ConflictException($"Mark distribution '{name}' already exists.");
        var entity = new MarkDistribution
        {
            Id = Guid.NewGuid(),
            Name = name,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await uow.MarkDistributions.AddAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
        return Map(entity, 0);
    }

    public async Task<MarkDistributionResponseDto> UpdateAsync(Guid id, UpdateMarkDistributionDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var entity = await uow.MarkDistributions.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Mark distribution '{id}' not found.");
        var name = dto.Name.Trim();
        if (await uow.MarkDistributions.NameExistsAsync(name, id, ct))
            throw new ConflictException($"Mark distribution '{name}' already exists.");
        entity.Name = name;
        if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;
        entity.UpdatedAt = DateTime.UtcNow;
        await uow.MarkDistributions.UpdateAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
        return Map(entity, 0);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var entity = await uow.MarkDistributions.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Mark distribution '{id}' not found.");
        var count = await uow.MarkDistributions.CountExamsUsingAsync(id, ct);
        if (count > 0)
            throw new AppException($"Mark distribution is in use by {count} exam(s) and cannot be deleted.", 400);
        await uow.MarkDistributions.DeleteAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
    }

    private MarkDistributionResponseDto Map(MarkDistribution x, int sl) => new()
    {
        Id = x.Id,
        Sl = sl,
        Branch = tenant.TenantName ?? string.Empty,
        Name = x.Name,
        IsActive = x.IsActive
    };

    private async Task Ready(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(tenant.SchemaName))
            throw new AppException("X-Tenant-ID header is required.", 400);
        await provisioner.EnsureExamMasterModuleAsync(tenant.SchemaName!, ct);
    }

    private HashSet<string> Roles()
        => http.HttpContext?.User.FindAll("role").Concat(http.HttpContext.User.FindAll(ClaimTypes.Role))
            .Select(x => x.Value).ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];

    private void Manage()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin))
            throw new ForbiddenException("Only Super Admin or School Admin can manage mark distributions.");
    }

    private void Read()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin) &&
            !r.Contains(AppConstants.Roles.Teacher))
            throw new ForbiddenException("You do not have access to mark distributions.");
    }
}
