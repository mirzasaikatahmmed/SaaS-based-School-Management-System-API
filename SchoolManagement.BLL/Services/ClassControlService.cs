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

public class ClassControlService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http) : IClassControlService
{
    public async Task<IReadOnlyList<ClassResponseDto>> GetAllAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        Read();
        var classes = await uow.ClassControls.GetAllWithSectionsAsync(ct);
        var result = new List<ClassResponseDto>();
        foreach (var c in classes)
            result.Add(await Map(c, ct));
        return result;
    }

    public async Task<ClassResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Read();
        var c = await uow.ClassControls.GetByIdWithSectionsAsync(id, ct)
            ?? throw new NotFoundException($"Class '{id}' not found.");
        return await Map(c, ct);
    }

    public async Task<ClassResponseDto> CreateAsync(CreateClassDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var name = dto.Name.Trim();
        if (await uow.ClassControls.NameExistsAsync(name, null, ct))
            throw new ConflictException($"Class '{name}' already exists.");

        if (dto.SectionIds.Count > 0)
        {
            var sections = await uow.ClassControls.GetSectionsByIdsAsync(dto.SectionIds, ct);
            if (sections.Count != dto.SectionIds.Distinct().Count())
                throw new AppException("One or more sections were not found.", 400);
        }

        var entity = new ClassEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            NumericName = dto.NumericName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await uow.ClassControls.AddAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);

        if (dto.SectionIds.Count > 0)
        {
            await uow.ClassControls.ReplaceClassSectionsAsync(entity.Id, dto.SectionIds, ct);
            await uow.SaveTenantChangesAsync(ct);
        }

        var created = await uow.ClassControls.GetByIdWithSectionsAsync(entity.Id, ct) ?? entity;
        return await Map(created, ct);
    }

    public async Task<ClassResponseDto> UpdateAsync(Guid id, UpdateClassDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var entity = await uow.ClassControls.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Class '{id}' not found.");

        var name = dto.Name.Trim();
        if (await uow.ClassControls.NameExistsAsync(name, id, ct))
            throw new ConflictException($"Class '{name}' already exists.");

        if (dto.SectionIds is { Count: > 0 })
        {
            var sections = await uow.ClassControls.GetSectionsByIdsAsync(dto.SectionIds, ct);
            if (sections.Count != dto.SectionIds.Distinct().Count())
                throw new AppException("One or more sections were not found.", 400);
        }

        entity.Name = name;
        entity.NumericName = dto.NumericName;
        if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;

        await uow.ClassControls.UpdateAsync(entity, ct);

        if (dto.SectionIds is not null)
            await uow.ClassControls.ReplaceClassSectionsAsync(entity.Id, dto.SectionIds, ct);

        await uow.SaveTenantChangesAsync(ct);

        var updated = await uow.ClassControls.GetByIdWithSectionsAsync(entity.Id, ct) ?? entity;
        return await Map(updated, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var entity = await uow.ClassControls.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Class '{id}' not found.");

        var studentCount = await uow.ClassControls.CountStudentsAsync(id, ct);
        if (studentCount > 0)
            throw new AppException($"Class has {studentCount} student(s) enrolled and cannot be deleted.", 400);

        await uow.ClassControls.DeleteAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
    }

    private async Task<ClassResponseDto> Map(ClassEntity c, CancellationToken ct) => new()
    {
        Id = c.Id,
        Name = c.Name,
        NumericName = c.NumericName,
        IsActive = c.IsActive,
        StudentCount = await uow.ClassControls.CountStudentsAsync(c.Id, ct),
        Sections = c.ClassSections.Select(cs => new SectionLookupDto
        {
            Id = cs.Section.Id,
            Name = cs.Section.Name,
            Capacity = cs.Section.Capacity
        }).ToList(),
        CreatedAt = c.CreatedAt
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
            throw new ForbiddenException("Only Super Admin or School Admin can manage classes.");
    }

    private void Read()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin) &&
            !r.Contains(AppConstants.Roles.Teacher) && !r.Contains(AppConstants.Roles.Student))
            throw new ForbiddenException("You do not have access to classes.");
    }
}
