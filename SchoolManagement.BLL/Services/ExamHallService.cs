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

public class ExamHallService(IUnitOfWork uow, ITenantContext tenant, ITenantSchemaProvisioner provisioner, IHttpContextAccessor http) : IExamHallService
{
    public async Task<IReadOnlyList<ExamHallResponseDto>> GetAllAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        Read();
        var items = await uow.ExamHalls.GetAllAsync(ct);
        return items.Select((x, i) => Map(x, i + 1)).ToList();
    }

    public async Task<ExamHallResponseDto> CreateAsync(CreateExamHallDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var hallNo = dto.HallNo.Trim();
        if (await uow.ExamHalls.HallNoExistsAsync(hallNo, null, ct))
            throw new ConflictException($"Exam hall '{hallNo}' already exists.");
        if (dto.NoOfSeats < 1)
            throw new AppException("Number of seats must be at least 1.", 400);
        var entity = new ExamHall
        {
            Id = Guid.NewGuid(),
            HallNo = hallNo,
            NoOfSeats = dto.NoOfSeats,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await uow.ExamHalls.AddAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
        return Map(entity, 0);
    }

    public async Task<ExamHallResponseDto> UpdateAsync(Guid id, UpdateExamHallDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var entity = await uow.ExamHalls.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Exam hall '{id}' not found.");
        var hallNo = dto.HallNo.Trim();
        if (await uow.ExamHalls.HallNoExistsAsync(hallNo, id, ct))
            throw new ConflictException($"Exam hall '{hallNo}' already exists.");
        if (dto.NoOfSeats < 1)
            throw new AppException("Number of seats must be at least 1.", 400);
        entity.HallNo = hallNo;
        entity.NoOfSeats = dto.NoOfSeats;
        if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;
        entity.UpdatedAt = DateTime.UtcNow;
        await uow.ExamHalls.UpdateAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
        return Map(entity, 0);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var entity = await uow.ExamHalls.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Exam hall '{id}' not found.");
        var count = await uow.ExamHalls.CountSchedulesUsingAsync(id, ct);
        if (count > 0)
            throw new AppException($"Exam hall is in use by {count} schedule subject(s) and cannot be deleted.", 400);
        await uow.ExamHalls.DeleteAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ExamHallLookupDto>> GetLookupAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        Read();
        return (await uow.ExamHalls.GetAllAsync(ct))
            .Where(h => h.IsActive)
            .Select(h => new ExamHallLookupDto { Id = h.Id, HallNo = h.HallNo, NoOfSeats = h.NoOfSeats })
            .ToList();
    }

    private ExamHallResponseDto Map(ExamHall x, int sl) => new()
    {
        Id = x.Id,
        Sl = sl,
        Branch = tenant.TenantName ?? string.Empty,
        HallNo = x.HallNo,
        NoOfSeats = x.NoOfSeats
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
            throw new ForbiddenException("Only Super Admin or School Admin can manage exam halls.");
    }

    private void Read()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin) &&
            !r.Contains(AppConstants.Roles.Teacher))
            throw new ForbiddenException("You do not have access to exam halls.");
    }
}
