using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.Marks;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class GradeRangeService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http) : IGradeRangeService
{
    public async Task<IReadOnlyList<GradeRangeDto>> GetAllAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        ReadAccess();
        var items = await uow.GradeRanges.GetAllAsync(ct);
        return items.Select(Map).ToList();
    }

    public async Task<GradeRangeDto> CreateAsync(CreateGradeRangeDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();

        var name = dto.GradeName.Trim();
        ValidateRange(dto.MinPercentage, dto.MaxPercentage);

        if (await uow.GradeRanges.NameExistsAsync(name, null, ct))
            throw new ConflictException($"Grade '{name}' already exists.");
        if (await uow.GradeRanges.OverlapsAsync(dto.MinPercentage, dto.MaxPercentage, null, ct))
            throw new ConflictException("Grade percentage range overlaps with an existing grade.");

        var entity = new GradeRange
        {
            Id = Guid.NewGuid(),
            GradeName = name,
            GradePoint = dto.GradePoint,
            MinPercentage = dto.MinPercentage,
            MaxPercentage = dto.MaxPercentage,
            Remarks = dto.Remarks?.Trim(),
            SortOrder = dto.SortOrder,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await uow.GradeRanges.AddAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
        return Map(entity);
    }

    public async Task<GradeRangeDto> UpdateAsync(Guid id, UpdateGradeRangeDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();

        var entity = await uow.GradeRanges.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Grade range '{id}' not found.");

        var name = dto.GradeName.Trim();
        ValidateRange(dto.MinPercentage, dto.MaxPercentage);

        if (await uow.GradeRanges.NameExistsAsync(name, id, ct))
            throw new ConflictException($"Grade '{name}' already exists.");
        if (await uow.GradeRanges.OverlapsAsync(dto.MinPercentage, dto.MaxPercentage, id, ct))
            throw new ConflictException("Grade percentage range overlaps with an existing grade.");

        entity.GradeName = name;
        entity.GradePoint = dto.GradePoint;
        entity.MinPercentage = dto.MinPercentage;
        entity.MaxPercentage = dto.MaxPercentage;
        entity.Remarks = dto.Remarks?.Trim();
        entity.SortOrder = dto.SortOrder;
        if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;
        entity.UpdatedAt = DateTime.UtcNow;

        await uow.GradeRanges.UpdateAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
        return Map(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();

        var entity = await uow.GradeRanges.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Grade range '{id}' not found.");

        await uow.GradeRanges.DeleteAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
    }

    private static void ValidateRange(decimal min, decimal max)
    {
        if (min < 0 || max > 100 || min > max)
            throw new AppException("Invalid percentage range. Min must be <= Max and within 0-100.", 400);
    }

    private GradeRangeDto Map(GradeRange g) => new()
    {
        Id = g.Id,
        Branch = tenant.TenantName ?? string.Empty,
        GradeName = g.GradeName,
        GradePoint = g.GradePoint,
        MinPercentage = g.MinPercentage,
        MaxPercentage = g.MaxPercentage,
        Remarks = g.Remarks,
        IsActive = g.IsActive,
        SortOrder = g.SortOrder,
        CreatedAt = g.CreatedAt
    };

    private async Task Ready(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(tenant.SchemaName))
            throw new AppException("X-Tenant-ID header is required.", 400);
        await provisioner.EnsureGradesAttendanceLibraryEventsModuleAsync(tenant.SchemaName!, ct);
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
            throw new ForbiddenException("Only Super Admin or School Admin can manage grade ranges.");
    }

    private void ReadAccess()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin) &&
            !r.Contains(AppConstants.Roles.Teacher))
            throw new ForbiddenException("You do not have access to grade ranges.");
    }
}
