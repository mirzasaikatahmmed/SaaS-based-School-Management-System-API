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

public class SubjectService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http) : ISubjectService
{
    public async Task<IReadOnlyList<SubjectResponseDto>> GetAllAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        Read();
        return (await uow.Subjects.GetAllAsync(ct)).Select(Map).ToList();
    }

    public async Task<SubjectResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Read();
        var x = await uow.Subjects.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Subject '{id}' not found.");
        return Map(x);
    }

    public async Task<SubjectResponseDto> CreateAsync(CreateSubjectDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var code = dto.Code.Trim();
        if (await uow.Subjects.CodeExistsAsync(code, null, ct))
            throw new ConflictException($"Subject code '{code}' already exists.");

        var subjectType = ValidateSubjectType(dto.SubjectType);
        var entity = new Subject
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            Code = code,
            Author = string.IsNullOrWhiteSpace(dto.Author) ? null : dto.Author.Trim(),
            SubjectType = subjectType,
            CanBeAdditional = dto.CanBeAdditional,
            IsContinuousAssessment = dto.IsContinuousAssessment,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await uow.Subjects.AddAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
        return Map(entity);
    }

    public async Task<SubjectResponseDto> UpdateAsync(Guid id, UpdateSubjectDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var entity = await uow.Subjects.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Subject '{id}' not found.");

        var code = dto.Code.Trim();
        if (await uow.Subjects.CodeExistsAsync(code, id, ct))
            throw new ConflictException($"Subject code '{code}' already exists.");

        entity.Name = dto.Name.Trim();
        entity.Code = code;
        entity.Author = string.IsNullOrWhiteSpace(dto.Author) ? null : dto.Author.Trim();
        entity.SubjectType = ValidateSubjectType(dto.SubjectType);
        if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;
        if (dto.CanBeAdditional.HasValue) entity.CanBeAdditional = dto.CanBeAdditional.Value;
        if (dto.IsContinuousAssessment.HasValue) entity.IsContinuousAssessment = dto.IsContinuousAssessment.Value;
        entity.UpdatedAt = DateTime.UtcNow;

        await uow.Subjects.UpdateAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
        return Map(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var entity = await uow.Subjects.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Subject '{id}' not found.");

        var usage = await uow.Subjects.CountAssignmentUsageAsync(id, ct);
        if (usage > 0)
            throw new AppException($"Subject is assigned to {usage} class-section combination(s) and cannot be deleted.", 400);

        await uow.Subjects.DeleteAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
    }

    private static string ValidateSubjectType(string subjectType)
        => SubjectTypes.All.FirstOrDefault(t => t.Equals(subjectType?.Trim(), StringComparison.OrdinalIgnoreCase))
           ?? throw new AppException("Invalid subject type.", 400);

    private static SubjectResponseDto Map(Subject x) => new()
    {
        Id = x.Id,
        Name = x.Name,
        Code = x.Code,
        Author = x.Author,
        SubjectType = x.SubjectType,
        CanBeAdditional = x.CanBeAdditional,
        IsContinuousAssessment = x.IsContinuousAssessment,
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
            throw new ForbiddenException("Only Super Admin or School Admin can manage subjects.");
    }

    private void Read()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin) &&
            !r.Contains(AppConstants.Roles.Teacher) && !r.Contains(AppConstants.Roles.Student))
            throw new ForbiddenException("You do not have access to subjects.");
    }
}
