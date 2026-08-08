using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.StudentDetails;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class DeactivateReasonService : IDeactivateReasonService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ITenantSchemaProvisioner _schemaProvisioner;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DeactivateReasonService(
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

    public async Task<IReadOnlyList<DeactivateReasonDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        EnsureTenant();
        EnsureCanManage();
        await EnsureReadyAsync(cancellationToken);

        var branch = _tenantContext.TenantName ?? string.Empty;
        var items = await _unitOfWork.DeactivateReasons.GetAllAsync(cancellationToken);
        return items.Select(r => Map(r, branch)).ToList();
    }

    public async Task<DeactivateReasonDto> CreateAsync(
        CreateDeactivateReasonDto dto,
        CancellationToken cancellationToken = default)
    {
        EnsureTenant();
        EnsureCanManage();
        await EnsureReadyAsync(cancellationToken);

        var reason = dto.Reason.Trim();
        if (await _unitOfWork.DeactivateReasons.ReasonExistsAsync(reason, null, cancellationToken))
            throw new ConflictException($"Reason '{reason}' already exists.");

        var entity = new DeactivateReason
        {
            Id = Guid.NewGuid(),
            Reason = reason,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.DeactivateReasons.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveTenantChangesAsync(cancellationToken);
        return Map(entity, _tenantContext.TenantName ?? string.Empty);
    }

    public async Task<DeactivateReasonDto> UpdateAsync(
        Guid id,
        UpdateDeactivateReasonDto dto,
        CancellationToken cancellationToken = default)
    {
        EnsureTenant();
        EnsureCanManage();
        await EnsureReadyAsync(cancellationToken);

        var entity = await _unitOfWork.DeactivateReasons.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Deactivate reason '{id}' not found.");

        var reason = dto.Reason.Trim();
        if (await _unitOfWork.DeactivateReasons.ReasonExistsAsync(reason, id, cancellationToken))
            throw new ConflictException($"Reason '{reason}' already exists.");

        entity.Reason = reason;
        entity.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.DeactivateReasons.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveTenantChangesAsync(cancellationToken);
        return Map(entity, _tenantContext.TenantName ?? string.Empty);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EnsureTenant();
        EnsureCanManage();
        await EnsureReadyAsync(cancellationToken);

        var entity = await _unitOfWork.DeactivateReasons.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Deactivate reason '{id}' not found.");

        var inUse = await _unitOfWork.DeactivateReasons.CountStudentsUsingAsync(id, cancellationToken);
        if (inUse > 0)
            throw new AppException($"Reason is in use by {inUse} student(s)", 400);

        await _unitOfWork.DeactivateReasons.DeleteAsync(entity, cancellationToken);
        await _unitOfWork.SaveTenantChangesAsync(cancellationToken);
    }

    private static DeactivateReasonDto Map(DeactivateReason entity, string branch) => new()
    {
        Id = entity.Id,
        Branch = branch,
        Reason = entity.Reason,
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAt
    };

    private async Task EnsureReadyAsync(CancellationToken cancellationToken)
    {
        await _schemaProvisioner.EnsureAdmissionModuleAsync(_tenantContext.SchemaName!, cancellationToken);
        await _schemaProvisioner.EnsureDeactivateReasonMasterAsync(_tenantContext.SchemaName!, cancellationToken);
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
        throw new ForbiddenException("Only Super Admin or School Admin can manage deactivate reasons.");
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
