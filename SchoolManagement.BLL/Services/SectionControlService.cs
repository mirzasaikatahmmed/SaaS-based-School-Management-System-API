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

public class SectionControlService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http) : ISectionControlService
{
    public async Task<IReadOnlyList<SectionResponseDto>> GetAllAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        Read();
        var sections = await uow.SectionControls.GetAllAsync(ct);
        var result = new List<SectionResponseDto>();
        foreach (var s in sections)
            result.Add(await Map(s, ct));
        return result;
    }

    public async Task<SectionResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Read();
        var s = await uow.SectionControls.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Section '{id}' not found.");
        return await Map(s, ct);
    }

    public async Task<SectionResponseDto> CreateAsync(CreateSectionDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var name = dto.Name.Trim();
        if (await uow.SectionControls.NameExistsAsync(name, null, ct))
            throw new ConflictException($"Section '{name}' already exists.");

        var entity = new Section
        {
            Id = Guid.NewGuid(),
            ClassId = null,
            Name = name,
            Capacity = dto.Capacity,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await uow.SectionControls.AddAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
        return await Map(entity, ct);
    }

    public async Task<SectionResponseDto> UpdateAsync(Guid id, UpdateSectionDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var entity = await uow.SectionControls.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Section '{id}' not found.");

        var name = dto.Name.Trim();
        if (await uow.SectionControls.NameExistsAsync(name, id, ct))
            throw new ConflictException($"Section '{name}' already exists.");

        entity.Name = name;
        entity.Capacity = dto.Capacity;
        if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;

        await uow.SectionControls.UpdateAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
        return await Map(entity, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var entity = await uow.SectionControls.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Section '{id}' not found.");

        var studentCount = await uow.SectionControls.CountStudentsAsync(id, ct);
        var classLinkCount = await uow.SectionControls.CountClassLinksAsync(id, ct);
        if (studentCount > 0 || classLinkCount > 0)
            throw new AppException(
                $"Section is in use by {studentCount} student(s) and {classLinkCount} class link(s) and cannot be deleted.",
                400);

        await uow.SectionControls.DeleteAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
    }

    private async Task<SectionResponseDto> Map(Section s, CancellationToken ct) => new()
    {
        Id = s.Id,
        Name = s.Name,
        Capacity = s.Capacity,
        IsActive = s.IsActive,
        StudentCount = await uow.SectionControls.CountStudentsAsync(s.Id, ct),
        ClassLinkCount = await uow.SectionControls.CountClassLinksAsync(s.Id, ct),
        CreatedAt = s.CreatedAt
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
            throw new ForbiddenException("Only Super Admin or School Admin can manage sections.");
    }

    private void Read()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin) &&
            !r.Contains(AppConstants.Roles.Teacher) && !r.Contains(AppConstants.Roles.Student))
            throw new ForbiddenException("You do not have access to sections.");
    }
}
