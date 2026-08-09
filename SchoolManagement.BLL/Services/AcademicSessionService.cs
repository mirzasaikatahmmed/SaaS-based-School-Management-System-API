using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.Settings;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class AcademicSessionService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http) : IAcademicSessionService
{
    public async Task<IReadOnlyList<AcademicSessionResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        Manage();
        return (await uow.AcademicSessions.GetAllAsync(cancellationToken)).Select(Map).ToList();
    }

    public async Task<AcademicSessionResponseDto?> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        Manage();
        var current = await uow.AcademicSessions.GetCurrentAsync(cancellationToken);
        return current is null ? null : Map(current);
    }

    public async Task<AcademicSessionResponseDto> CreateAsync(CreateAcademicSessionDto dto, CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        Manage();
        var name = dto.Name.Trim();
        if (await uow.AcademicSessions.NameExistsAsync(name, null, cancellationToken))
            throw new ConflictException($"Session '{name}' already exists.");

        await uow.BeginTenantTransactionAsync(cancellationToken);
        try
        {
            if (dto.IsSelected)
                await uow.AcademicSessions.ClearSelectedAsync(cancellationToken);

            var session = new AcademicSession
            {
                Id = Guid.NewGuid(),
                Name = name,
                IsSelected = dto.IsSelected,
                CreatedAt = DateTime.UtcNow
            };
            await uow.AcademicSessions.AddAsync(session, cancellationToken);
            await uow.SaveTenantChangesAsync(cancellationToken);
            await uow.CommitTenantTransactionAsync(cancellationToken);
            return Map(session);
        }
        catch
        {
            await uow.RollbackTenantTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<AcademicSessionResponseDto> UpdateAsync(Guid id, UpdateAcademicSessionDto dto, CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        Manage();
        var session = await uow.AcademicSessions.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Session '{id}' not found.");

        await uow.BeginTenantTransactionAsync(cancellationToken);
        try
        {
            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                var name = dto.Name.Trim();
                if (await uow.AcademicSessions.NameExistsAsync(name, id, cancellationToken))
                    throw new ConflictException($"Session '{name}' already exists.");
                session.Name = name;
            }

            if (dto.IsSelected == true)
            {
                await uow.AcademicSessions.ClearSelectedAsync(cancellationToken);
                session.IsSelected = true;
            }
            else if (dto.IsSelected == false)
            {
                session.IsSelected = false;
            }

            await uow.AcademicSessions.UpdateAsync(session, cancellationToken);
            await uow.SaveTenantChangesAsync(cancellationToken);
            await uow.CommitTenantTransactionAsync(cancellationToken);
            return Map(session);
        }
        catch
        {
            await uow.RollbackTenantTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        Manage();
        var session = await uow.AcademicSessions.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Session '{id}' not found.");

        if (int.TryParse(session.Name, out var year))
        {
            var count = await uow.AcademicSessions.CountStudentsForYearAsync(year, cancellationToken);
            if (count > 0)
                throw new ConflictException($"Session '{session.Name}' has {count} student record(s) and cannot be deleted.");
        }

        await uow.AcademicSessions.DeleteAsync(session, cancellationToken);
        await uow.SaveTenantChangesAsync(cancellationToken);
    }

    private static AcademicSessionResponseDto Map(AcademicSession s) => new()
    {
        Id = s.Id,
        Name = s.Name,
        IsSelected = s.IsSelected,
        CreatedAt = s.CreatedAt
    };

    private async Task Ready(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(tenant.SchemaName))
            throw new AppException("X-Tenant-ID header is required.", 400);
        await provisioner.EnsureSettingsModuleAsync(tenant.SchemaName!, ct);
    }

    private HashSet<string> Roles() =>
        http.HttpContext?.User.FindAll("role").Concat(http.HttpContext.User.FindAll(ClaimTypes.Role))
            .Select(x => x.Value).ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];

    private void Manage()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin))
            throw new ForbiddenException("Only Super Admin or School Admin can manage sessions.");
    }
}
