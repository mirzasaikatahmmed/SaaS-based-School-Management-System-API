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

public class ExamTermService(IUnitOfWork uow, ITenantContext tenant, ITenantSchemaProvisioner provisioner, IHttpContextAccessor http) : IExamTermService
{
    public async Task<IReadOnlyList<ExamTermResponseDto>> GetAllAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        Read();
        var items = await uow.ExamTerms.GetAllAsync(ct);
        return items.Select((x, i) => Map(x, i + 1)).ToList();
    }

    public async Task<ExamTermResponseDto> CreateAsync(CreateExamTermDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var name = dto.Name.Trim();
        if (await uow.ExamTerms.NameExistsAsync(name, null, ct))
            throw new ConflictException($"Exam term '{name}' already exists.");
        var entity = new ExamTerm
        {
            Id = Guid.NewGuid(),
            Name = name,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await uow.ExamTerms.AddAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
        return Map(entity, 0);
    }

    public async Task<ExamTermResponseDto> UpdateAsync(Guid id, UpdateExamTermDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var entity = await uow.ExamTerms.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Exam term '{id}' not found.");
        var name = dto.Name.Trim();
        if (await uow.ExamTerms.NameExistsAsync(name, id, ct))
            throw new ConflictException($"Exam term '{name}' already exists.");
        entity.Name = name;
        if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;
        entity.UpdatedAt = DateTime.UtcNow;
        await uow.ExamTerms.UpdateAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
        return Map(entity, 0);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var entity = await uow.ExamTerms.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Exam term '{id}' not found.");
        var count = await uow.ExamTerms.CountExamsUsingAsync(id, ct);
        if (count > 0)
            throw new AppException($"Exam term is in use by {count} exam(s) and cannot be deleted.", 400);
        await uow.ExamTerms.DeleteAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
    }

    private ExamTermResponseDto Map(ExamTerm x, int sl) => new()
    {
        Id = x.Id,
        Sl = sl,
        Branch = tenant.TenantName ?? string.Empty,
        Name = x.Name,
        IsActive = x.IsActive,
        CreatedAt = x.CreatedAt
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
            throw new ForbiddenException("Only Super Admin or School Admin can manage exam terms.");
    }

    private void Read()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin) &&
            !r.Contains(AppConstants.Roles.Teacher))
            throw new ForbiddenException("You do not have access to exam terms.");
    }
}
