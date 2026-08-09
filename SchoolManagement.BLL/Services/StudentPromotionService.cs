using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.Academic;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class StudentPromotionService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http) : IStudentPromotionService
{
    public async Task<PromotionStudentListResponseDto> GetStudentsAsync(PromotionFilterDto filter, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();

        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize is < 1 or > 500 ? 50 : filter.PageSize;

        var (items, total) = await uow.Students.SearchAsync(new StudentSearchFilter
        {
            ClassId = filter.ClassId,
            SectionId = filter.SectionId,
            AcademicYear = filter.AcademicYear,
            IsActive = true,
            Page = page,
            PageSize = pageSize
        }, ct);

        return new PromotionStudentListResponseDto
        {
            Data = items.Select(Map).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize)
        };
    }

    public async Task<ProcessPromotionResultDto> ProcessAsync(ProcessPromotionDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();

        if (dto.Items.Count == 0)
            throw new AppException("No students provided for promotion.", 400);

        var promotedBy = CurrentUser();
        var results = new List<PromotionResultItemDto>();

        foreach (var item in dto.Items)
        {
            try
            {
                var status = PromotionStatuses.All.FirstOrDefault(s => s.Equals(item.Status.Trim(), StringComparison.OrdinalIgnoreCase))
                    ?? throw new AppException($"Invalid promotion status '{item.Status}'.", 400);

                var student = await uow.Students.GetByIdAsync(item.StudentId, ct)
                    ?? throw new NotFoundException($"Student '{item.StudentId}' not found.");

                var fromAcademicYear = student.AcademicYear;
                var fromClassId = student.ClassId;
                var fromSectionId = student.SectionId;
                var fromRoll = student.Roll;

                int toAcademicYear;
                Guid? toClassId;
                Guid? toSectionId;
                string? toRoll;

                switch (status)
                {
                    case PromotionStatuses.Promoted:
                        if (!item.ToClassId.HasValue || !item.ToSectionId.HasValue || !item.ToAcademicYear.HasValue)
                            throw new AppException("ToClassId, ToSectionId, and ToAcademicYear are required for Promoted status.", 400);

                        toAcademicYear = item.ToAcademicYear.Value;
                        toClassId = item.ToClassId.Value;
                        toSectionId = item.ToSectionId.Value;
                        toRoll = string.IsNullOrWhiteSpace(item.ToRoll) ? fromRoll : item.ToRoll.Trim();

                        student.ClassId = toClassId;
                        student.SectionId = toSectionId;
                        student.AcademicYear = toAcademicYear;
                        student.Roll = toRoll;
                        break;

                    case PromotionStatuses.Running:
                        if (!item.ToAcademicYear.HasValue)
                            throw new AppException("ToAcademicYear is required for Running status.", 400);

                        toAcademicYear = item.ToAcademicYear.Value;
                        toClassId = fromClassId;
                        toSectionId = fromSectionId;
                        toRoll = fromRoll;

                        student.AcademicYear = toAcademicYear;
                        break;

                    case PromotionStatuses.Left:
                    case PromotionStatuses.Alumni:
                        toAcademicYear = fromAcademicYear;
                        toClassId = fromClassId;
                        toSectionId = fromSectionId;
                        toRoll = fromRoll;

                        student.IsActive = false;
                        student.DeactivatedAt = DateTime.UtcNow;
                        student.DeactivateReason = status;
                        break;

                    default:
                        throw new AppException($"Unsupported promotion status '{status}'.", 400);
                }

                student.UpdatedAt = DateTime.UtcNow;
                await uow.Students.UpdateAsync(student, ct);

                var history = new StudentPromotion
                {
                    Id = Guid.NewGuid(),
                    StudentId = student.Id,
                    FromAcademicYear = fromAcademicYear,
                    FromClassId = fromClassId,
                    FromSectionId = fromSectionId,
                    FromRoll = fromRoll,
                    ToAcademicYear = toAcademicYear,
                    ToClassId = toClassId,
                    ToSectionId = toSectionId,
                    ToRoll = toRoll,
                    Status = status,
                    CurrentDueAmount = 0,
                    CarryForwardDue = item.CarryForwardDue,
                    PromotedBy = promotedBy,
                    PromotedAt = DateTime.UtcNow
                };
                await uow.StudentPromotions.AddAsync(history, ct);

                results.Add(new PromotionResultItemDto { StudentId = item.StudentId, Status = status, Success = true });
            }
            catch (AppException ex)
            {
                results.Add(new PromotionResultItemDto
                {
                    StudentId = item.StudentId,
                    Status = item.Status,
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        await uow.SaveTenantChangesAsync(ct);

        return new ProcessPromotionResultDto
        {
            ProcessedCount = results.Count(r => r.Success),
            Results = results
        };
    }

    private static PromotionStudentListItemDto Map(Student s) => new()
    {
        Id = s.Id,
        RegisterNo = s.RegisterNo,
        Roll = s.Roll,
        Name = string.IsNullOrWhiteSpace(s.LastName) ? s.FirstName.Trim() : $"{s.FirstName.Trim()} {s.LastName.Trim()}",
        ClassId = s.ClassId,
        ClassName = s.Class?.Name,
        SectionId = s.SectionId,
        SectionName = s.Section?.Name,
        AcademicYear = s.AcademicYear
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
            throw new ForbiddenException("Only Super Admin or School Admin can manage student promotions.");
    }

    private Guid CurrentUser()
    {
        var c = http.HttpContext?.User.FindFirst(AppConstants.Claims.UserId)
            ?? http.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        if (c is null || !Guid.TryParse(c.Value, out var id)) throw new UnauthorizedException();
        return id;
    }
}
