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

public class ExamService(IUnitOfWork uow, ITenantContext tenant, ITenantSchemaProvisioner provisioner, IHttpContextAccessor http) : IExamService
{
    public async Task<IReadOnlyList<ExamListItemDto>> GetAllAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        Read();
        var items = await uow.Exams.GetAllAsync(ct);
        if (IsStudent())
            items = items.Where(e => e.IsPublished).ToList();
        return items.Select((x, i) => MapList(x, i + 1)).ToList();
    }

    public async Task<ExamResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Read();
        var exam = await uow.Exams.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Exam '{id}' not found.");
        if (IsStudent() && !exam.IsPublished)
            throw new ForbiddenException("Exam is not published.");
        return MapDetail(exam, 0);
    }

    public async Task<ExamResponseDto> CreateAsync(CreateExamDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var name = dto.Name.Trim();
        if (await uow.Exams.NameExistsAsync(name, null, ct))
            throw new ConflictException($"Exam '{name}' already exists.");
        await ValidateTermAndDistributions(dto.ExamTermId, dto.MarkDistributionIds, ct);

        var entity = new Exam
        {
            Id = Guid.NewGuid(),
            Name = name,
            ExamTermId = dto.ExamTermId,
            ExamType = string.IsNullOrWhiteSpace(dto.ExamType) ? null : dto.ExamType.Trim(),
            Remarks = string.IsNullOrWhiteSpace(dto.Remarks) ? null : dto.Remarks.Trim(),
            IsPublished = dto.IsPublished,
            IsResultPublished = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await uow.Exams.AddAsync(entity, ct);
        await SaveMarkDistributions(entity.Id, dto.MarkDistributionIds, ct);
        await uow.SaveTenantChangesAsync(ct);
        return MapDetail(await uow.Exams.GetByIdAsync(entity.Id, ct) ?? entity, 0);
    }

    public async Task<ExamResponseDto> UpdateAsync(Guid id, UpdateExamDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var entity = await uow.Exams.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Exam '{id}' not found.");
        var name = dto.Name.Trim();
        if (await uow.Exams.NameExistsAsync(name, id, ct))
            throw new ConflictException($"Exam '{name}' already exists.");
        await ValidateTermAndDistributions(dto.ExamTermId, dto.MarkDistributionIds, ct);

        entity.Name = name;
        entity.ExamTermId = dto.ExamTermId;
        entity.ExamType = string.IsNullOrWhiteSpace(dto.ExamType) ? null : dto.ExamType.Trim();
        entity.Remarks = string.IsNullOrWhiteSpace(dto.Remarks) ? null : dto.Remarks.Trim();
        entity.IsPublished = dto.IsPublished;
        entity.UpdatedAt = DateTime.UtcNow;

        await uow.Exams.UpdateAsync(entity, ct);
        await SaveMarkDistributions(id, dto.MarkDistributionIds, ct);
        await uow.SaveTenantChangesAsync(ct);
        return MapDetail(await uow.Exams.GetByIdAsync(id, ct) ?? entity, 0);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var entity = await uow.Exams.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Exam '{id}' not found.");
        var scheduleCount = await uow.Exams.CountSchedulesAsync(id, ct);
        if (scheduleCount > 0)
            throw new AppException($"Exam has {scheduleCount} schedule(s) and cannot be deleted.", 400);
        var markCount = await uow.Exams.CountMarkEntriesAsync(id, ct);
        if (markCount > 0)
            throw new AppException($"Exam has {markCount} mark entr(ies) and cannot be deleted.", 400);
        await uow.Exams.DeleteAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
    }

    public async Task<ExamResponseDto> TogglePublishAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var entity = await uow.Exams.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Exam '{id}' not found.");
        entity.IsPublished = !entity.IsPublished;
        entity.UpdatedAt = DateTime.UtcNow;
        await uow.Exams.UpdateAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
        return MapDetail(await uow.Exams.GetByIdAsync(id, ct) ?? entity, 0);
    }

    public async Task<ExamResponseDto> TogglePublishResultAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var entity = await uow.Exams.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Exam '{id}' not found.");
        entity.IsResultPublished = !entity.IsResultPublished;
        entity.UpdatedAt = DateTime.UtcNow;
        await uow.Exams.UpdateAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
        return MapDetail(await uow.Exams.GetByIdAsync(id, ct) ?? entity, 0);
    }

    public async Task<IReadOnlyList<ExamLookupDto>> GetLookupAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        Read();
        var items = await uow.Exams.GetAllAsync(ct);
        if (IsStudent())
            items = items.Where(e => e.IsPublished).ToList();
        return items.Select(e => new ExamLookupDto
        {
            Id = e.Id,
            Name = e.Name,
            TermName = e.ExamTerm?.Name
        }).ToList();
    }

    private async Task ValidateTermAndDistributions(Guid? termId, IReadOnlyList<Guid> distributionIds, CancellationToken ct)
    {
        if (termId.HasValue && await uow.ExamTerms.GetByIdAsync(termId.Value, ct) is null)
            throw new NotFoundException($"Exam term '{termId}' not found.");
        foreach (var id in distributionIds.Distinct())
        {
            if (await uow.MarkDistributions.GetByIdAsync(id, ct) is null)
                throw new NotFoundException($"Mark distribution '{id}' not found.");
        }
    }

    private async Task SaveMarkDistributions(Guid examId, IReadOnlyList<Guid> distributionIds, CancellationToken ct)
    {
        var items = distributionIds.Distinct().Select(id => new ExamMarkDistribution
        {
            Id = Guid.NewGuid(),
            ExamId = examId,
            MarkDistributionId = id
        }).ToList();
        await uow.Exams.ReplaceMarkDistributionsAsync(examId, items, ct);
    }

    private ExamListItemDto MapList(Exam x, int sl) => new()
    {
        Id = x.Id,
        Sl = sl,
        Branch = tenant.TenantName ?? string.Empty,
        ExamName = x.Name,
        ExamType = x.ExamType,
        Term = x.ExamTerm?.Name,
        MarkDistributions = x.MarkDistributions.Select(d => d.MarkDistribution.Name).ToList(),
        IsPublished = x.IsPublished,
        IsResultPublished = x.IsResultPublished,
        Remarks = x.Remarks
    };

    private ExamResponseDto MapDetail(Exam x, int sl)
    {
        var list = MapList(x, sl);
        return new ExamResponseDto
        {
            Id = list.Id,
            Sl = list.Sl,
            Branch = list.Branch,
            ExamName = list.ExamName,
            ExamType = list.ExamType,
            Term = list.Term,
            MarkDistributions = list.MarkDistributions,
            IsPublished = list.IsPublished,
            IsResultPublished = list.IsResultPublished,
            Remarks = list.Remarks,
            ExamTermId = x.ExamTermId,
            MarkDistributionIds = x.MarkDistributions.Select(d => d.MarkDistributionId).ToList()
        };
    }

    private async Task Ready(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(tenant.SchemaName))
            throw new AppException("X-Tenant-ID header is required.", 400);
        await provisioner.EnsureExamMasterModuleAsync(tenant.SchemaName!, ct);
    }

    private HashSet<string> Roles()
        => http.HttpContext?.User.FindAll("role").Concat(http.HttpContext.User.FindAll(ClaimTypes.Role))
            .Select(x => x.Value).ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];

    private bool IsStudent() => Roles().Contains(AppConstants.Roles.Student);

    private void Manage()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin))
            throw new ForbiddenException("Only Super Admin or School Admin can manage exams.");
    }

    private void Read()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin) &&
            !r.Contains(AppConstants.Roles.Teacher) && !r.Contains(AppConstants.Roles.Student))
            throw new ForbiddenException("You do not have access to exams.");
    }
}
