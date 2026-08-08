using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.StudentCategory;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.BLL.Mappings;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class StudentCategoryService : IStudentCategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ITenantSchemaProvisioner _schemaProvisioner;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public StudentCategoryService(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ITenantSchemaProvisioner schemaProvisioner,
        IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _schemaProvisioner = schemaProvisioner;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IReadOnlyList<StudentCategoryResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        EnsureTenant();
        EnsureCanRead();
        await EnsureReadyAsync(cancellationToken);

        var branch = _tenantContext.TenantName ?? string.Empty;
        var items = await _unitOfWork.StudentCategories.GetAllAsync(cancellationToken);
        return items.Select((c, i) => AdmissionMappings.ToCategoryDto(c, i + 1, branch)).ToList();
    }

    public async Task<StudentCategoryResponseDto> CreateAsync(
        CreateStudentCategoryDto dto,
        CancellationToken cancellationToken = default)
    {
        EnsureTenant();
        EnsureCanManage();
        await EnsureReadyAsync(cancellationToken);

        var name = dto.Name.Trim().ToUpperInvariant();
        if (await _unitOfWork.StudentCategories.NameExistsAsync(name, null, cancellationToken))
            throw new ConflictException($"Category '{name}' already exists.");

        var entity = new StudentCategory
        {
            Id = Guid.NewGuid(),
            Name = name,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.StudentCategories.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveTenantChangesAsync(cancellationToken);

        return AdmissionMappings.ToCategoryDto(entity, 1, _tenantContext.TenantName ?? string.Empty);
    }

    public async Task<StudentCategoryResponseDto> UpdateAsync(
        Guid id,
        UpdateStudentCategoryDto dto,
        CancellationToken cancellationToken = default)
    {
        EnsureTenant();
        EnsureCanManage();
        await EnsureReadyAsync(cancellationToken);

        var entity = await _unitOfWork.StudentCategories.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Category '{id}' not found.");

        var name = dto.Name.Trim().ToUpperInvariant();
        if (await _unitOfWork.StudentCategories.NameExistsAsync(name, id, cancellationToken))
            throw new ConflictException($"Category '{name}' already exists.");

        entity.Name = name;
        await _unitOfWork.StudentCategories.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveTenantChangesAsync(cancellationToken);

        return AdmissionMappings.ToCategoryDto(entity, 1, _tenantContext.TenantName ?? string.Empty);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EnsureTenant();
        EnsureCanManage();
        await EnsureReadyAsync(cancellationToken);

        var entity = await _unitOfWork.StudentCategories.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Category '{id}' not found.");

        var inUse = await _unitOfWork.StudentCategories.CountStudentsUsingAsync(id, cancellationToken);
        if (inUse > 0)
            throw new AppException(
                $"Category is in use by {inUse} student(s) and cannot be deleted",
                400);

        await _unitOfWork.StudentCategories.DeleteAsync(entity, cancellationToken);
        await _unitOfWork.SaveTenantChangesAsync(cancellationToken);
    }

    private async Task EnsureReadyAsync(CancellationToken cancellationToken)
    {
        await _schemaProvisioner.EnsureAdmissionModuleAsync(_tenantContext.SchemaName!, cancellationToken);
    }

    private void EnsureTenant()
    {
        if (string.IsNullOrEmpty(_tenantContext.SchemaName))
            throw new AppException("X-Tenant-ID header is required.", 400);
    }

    private void EnsureCanManage()
    {
        var roles = GetCurrentRoles();
        if (roles.Contains(AppConstants.Roles.SuperAdmin) || roles.Contains(AppConstants.Roles.Admin))
            return;
        throw new ForbiddenException("Only Super Admin or School Admin can manage student categories.");
    }

    private void EnsureCanRead()
    {
        var roles = GetCurrentRoles();
        if (roles.Contains(AppConstants.Roles.SuperAdmin) ||
            roles.Contains(AppConstants.Roles.Admin) ||
            roles.Contains(AppConstants.Roles.Teacher))
            return;
        throw new ForbiddenException("You do not have access to student categories.");
    }

    private HashSet<string> GetCurrentRoles()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user is null)
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var roles = user.FindAll("role")
            .Concat(user.FindAll(ClaimTypes.Role))
            .Select(c => c.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var rolesCsv = user.FindFirst(AppConstants.Claims.Roles)?.Value;
        if (!string.IsNullOrWhiteSpace(rolesCsv))
        {
            foreach (var r in rolesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                roles.Add(r);
        }

        return roles;
    }
}
