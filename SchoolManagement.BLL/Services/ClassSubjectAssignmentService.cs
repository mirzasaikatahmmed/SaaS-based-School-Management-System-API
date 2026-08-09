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

public class ClassSubjectAssignmentService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http) : IClassSubjectAssignmentService
{
    public async Task<IReadOnlyList<ClassSubjectAssignmentResponseDto>> GetAllAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        Read();
        return (await uow.ClassSubjectAssignments.GetAllAsync(ct)).Select(Map).ToList();
    }

    public async Task<ClassSubjectAssignmentResponseDto> GetByClassSectionAsync(Guid classId, Guid sectionId, CancellationToken ct = default)
    {
        await Ready(ct);
        Read();
        var x = await uow.ClassSubjectAssignments.GetByClassSectionAsync(classId, sectionId, ct)
            ?? throw new NotFoundException("No subject assignment found for this class and section.");
        return Map(x);
    }

    public async Task<ClassSubjectAssignmentResponseDto> UpsertAsync(UpsertClassSubjectAssignmentDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();

        var inputs = NormalizeItems(dto);
        if (inputs.Count == 0)
            throw new AppException("At least one subject must be provided.", 400);

        foreach (var item in inputs)
        {
            if (await uow.Subjects.GetByIdAsync(item.SubjectId, ct) is null)
                throw new NotFoundException($"Subject '{item.SubjectId}' not found.");
            if (item.IsElective && string.IsNullOrWhiteSpace(item.ElectiveGroup))
                throw new AppException("Elective subjects require electiveGroup (e.g. \"4th\").", 400);
        }

        // Within each elective group there must be at least 2 options to be useful — warn soft via allow 1+
        var existing = await uow.ClassSubjectAssignments.GetByClassSectionAsync(dto.ClassId, dto.SectionId, ct);
        Guid assignmentId;
        if (existing is not null)
        {
            existing.UpdatedAt = DateTime.UtcNow;
            await uow.ClassSubjectAssignments.UpdateAsync(existing, ct);
            assignmentId = existing.Id;
        }
        else
        {
            var entity = new ClassSubjectAssignment
            {
                Id = Guid.NewGuid(),
                ClassId = dto.ClassId,
                SectionId = dto.SectionId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await uow.ClassSubjectAssignments.AddAsync(entity, ct);
            assignmentId = entity.Id;
        }

        var items = inputs.Select(i => new ClassSubjectAssignmentItem
        {
            Id = Guid.NewGuid(),
            AssignmentId = assignmentId,
            SubjectId = i.SubjectId,
            IsElective = i.IsElective,
            ElectiveGroup = i.IsElective ? i.ElectiveGroup!.Trim() : null
        }).ToList();

        await uow.ClassSubjectAssignments.ReplaceItemsAsync(assignmentId, items, ct);
        await uow.SaveTenantChangesAsync(ct);

        var result = await uow.ClassSubjectAssignments.GetByIdAsync(assignmentId, ct)
            ?? throw new AppException("Failed to load saved subject assignment.", 500);
        return Map(result);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var x = await uow.ClassSubjectAssignments.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Subject assignment '{id}' not found.");
        await uow.ClassSubjectAssignments.DeleteAsync(x, ct);
        await uow.SaveTenantChangesAsync(ct);
    }

    private static List<ClassSubjectItemInputDto> NormalizeItems(UpsertClassSubjectAssignmentDto dto)
    {
        if (dto.Items.Count > 0)
            return dto.Items
                .GroupBy(i => i.SubjectId)
                .Select(g => g.Last())
                .ToList();

        return dto.SubjectIds.Distinct().Select(id => new ClassSubjectItemInputDto
        {
            SubjectId = id,
            IsElective = false
        }).ToList();
    }

    private static ClassSubjectAssignmentResponseDto Map(ClassSubjectAssignment x) => new()
    {
        Id = x.Id,
        ClassId = x.ClassId,
        ClassName = x.Class?.Name ?? string.Empty,
        SectionId = x.SectionId,
        SectionName = x.Section?.Name ?? string.Empty,
        Subjects = x.Items.Select(i => new AssignedSubjectDto
        {
            Id = i.Subject.Id,
            Name = i.Subject.Name,
            Code = i.Subject.Code,
            IsElective = i.IsElective,
            ElectiveGroup = i.ElectiveGroup
        }).ToList(),
        CreatedAt = x.CreatedAt,
        UpdatedAt = x.UpdatedAt
    };

    private async Task Ready(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(tenant.SchemaName))
            throw new AppException("X-Tenant-ID header is required.", 400);
        await provisioner.EnsureSettingsModuleAsync(tenant.SchemaName!, ct);
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
            throw new ForbiddenException("Only Super Admin or School Admin can manage subject assignments.");
    }

    private void Read()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin) &&
            !r.Contains(AppConstants.Roles.Teacher) && !r.Contains(AppConstants.Roles.Student))
            throw new ForbiddenException("You do not have access to subject assignments.");
    }
}
