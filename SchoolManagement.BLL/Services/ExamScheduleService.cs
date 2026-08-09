using System.Globalization;
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

public class ExamScheduleService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http) : IExamScheduleService
{
    private static readonly CultureInfo DateCulture = CultureInfo.InvariantCulture;

    public async Task<IReadOnlyList<ExamScheduleResponseDto>> GetFilteredAsync(ExamScheduleFilterDto filter, CancellationToken ct = default)
    {
        await Ready(ct);
        Read();
        var items = await uow.ExamSchedules.GetFilteredAsync(filter.ClassId, filter.SectionId, ct);
        if (IsStudent())
            items = items.Where(s => s.Exam.IsPublished).ToList();
        return items.Select((x, i) => MapList(x, i + 1)).ToList();
    }

    public async Task<ExamScheduleDetailDto> GetDetailAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Read();
        var schedule = await uow.ExamSchedules.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Exam schedule '{id}' not found.");
        if (IsStudent() && !schedule.Exam.IsPublished)
            throw new ForbiddenException("Exam schedule is not available.");
        return MapDetail(schedule);
    }

    public async Task<ExamScheduleDetailDto> CreateAsync(CreateExamScheduleDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        await EnsureExamExists(dto.ExamId, ct);
        if (await uow.ExamSchedules.ExistsUniqueAsync(dto.ExamId, dto.ClassId, dto.SectionId, null, ct))
            throw new ConflictException("An exam schedule already exists for this exam, class, and section.");

        var subjects = await ResolveSubjects(dto, ct);
        ValidateSubjects(subjects);

        var entity = new ExamSchedule
        {
            Id = Guid.NewGuid(),
            ExamId = dto.ExamId,
            ClassId = dto.ClassId,
            SectionId = dto.SectionId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await uow.ExamSchedules.AddAsync(entity, ct);
        await uow.ExamSchedules.ReplaceSubjectsAsync(entity.Id, BuildSubjectEntities(entity.Id, subjects), ct);
        await uow.SaveTenantChangesAsync(ct);

        var saved = await uow.ExamSchedules.GetByIdAsync(entity.Id, ct)
            ?? throw new AppException("Failed to load saved exam schedule.", 500);
        return MapDetail(saved);
    }

    public async Task<ExamScheduleDetailDto> UpdateAsync(Guid id, CreateExamScheduleDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var entity = await uow.ExamSchedules.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Exam schedule '{id}' not found.");
        await EnsureExamExists(dto.ExamId, ct);
        if (await uow.ExamSchedules.ExistsUniqueAsync(dto.ExamId, dto.ClassId, dto.SectionId, id, ct))
            throw new ConflictException("An exam schedule already exists for this exam, class, and section.");

        var subjects = await ResolveSubjects(dto, ct);
        ValidateSubjects(subjects);

        entity.ExamId = dto.ExamId;
        entity.ClassId = dto.ClassId;
        entity.SectionId = dto.SectionId;
        entity.UpdatedAt = DateTime.UtcNow;

        await uow.ExamSchedules.UpdateAsync(entity, ct);
        await uow.ExamSchedules.ReplaceSubjectsAsync(id, BuildSubjectEntities(id, subjects), ct);
        await uow.SaveTenantChangesAsync(ct);

        var saved = await uow.ExamSchedules.GetByIdAsync(id, ct)
            ?? throw new AppException("Failed to load saved exam schedule.", 500);
        return MapDetail(saved);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var entity = await uow.ExamSchedules.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Exam schedule '{id}' not found.");
        await uow.ExamSchedules.DeleteAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
    }

    private async Task EnsureExamExists(Guid examId, CancellationToken ct)
    {
        if (await uow.Exams.GetByIdAsync(examId, ct) is null)
            throw new NotFoundException($"Exam '{examId}' not found.");
    }

    private async Task<List<ExamScheduleSubjectDto>> ResolveSubjects(CreateExamScheduleDto dto, CancellationToken ct)
    {
        if (dto.Subjects.Count > 0)
            return dto.Subjects;

        var assignment = await uow.ClassSubjectAssignments.GetByClassSectionAsync(dto.ClassId, dto.SectionId, ct);
        if (assignment is null || assignment.Items.Count == 0)
            throw new AppException("No subjects assigned to this class-section.", 400);

        if (!dto.StartingDate.HasValue || !dto.StartingTime.HasValue || !dto.ExamDurationMinutes.HasValue)
            throw new AppException("StartingDate, StartingTime, and ExamDurationMinutes are required when subjects are not provided.", 400);

        var startDate = DateTime.SpecifyKind(dto.StartingDate.Value.Date, DateTimeKind.Utc);
        var startTime = dto.StartingTime.Value;
        var endTime = startTime.Add(TimeSpan.FromMinutes(dto.ExamDurationMinutes.Value));

        return assignment.Items
            .OrderBy(i => i.Subject.Name)
            .Select((item, index) => new ExamScheduleSubjectDto
            {
                SubjectId = item.SubjectId,
                SubjectName = item.Subject.Name,
                ExamDate = startDate.AddDays(index),
                StartingTime = startTime,
                EndingTime = endTime,
                HallId = dto.DefaultHallId,
                WrittenFullMark = dto.WrittenFullMark,
                WrittenPassMark = dto.WrittenPassMark,
                SortOrder = index
            }).ToList();
    }

    private static void ValidateSubjects(IReadOnlyList<ExamScheduleSubjectDto> subjects)
    {
        if (subjects.Count == 0)
            throw new AppException("At least one subject is required.", 400);
        foreach (var s in subjects)
        {
            if (s.EndingTime <= s.StartingTime)
                throw new AppException("Subject ending time must be after starting time.", 400);
        }
    }

    private static IEnumerable<ExamScheduleSubject> BuildSubjectEntities(Guid scheduleId, IReadOnlyList<ExamScheduleSubjectDto> subjects)
        => subjects.Select((s, i) => new ExamScheduleSubject
        {
            Id = Guid.NewGuid(),
            ScheduleId = scheduleId,
            SubjectId = s.SubjectId,
            ExamDate = DateTime.SpecifyKind(s.ExamDate.Date, DateTimeKind.Utc),
            StartingTime = s.StartingTime,
            EndingTime = s.EndingTime,
            HallId = s.HallId,
            WrittenFullMark = s.WrittenFullMark,
            WrittenPassMark = s.WrittenPassMark,
            SortOrder = s.SortOrder != 0 ? s.SortOrder : i,
            CreatedAt = DateTime.UtcNow
        });

    private ExamScheduleResponseDto MapList(ExamSchedule x, int sl) => new()
    {
        Id = x.Id,
        Sl = sl,
        Branch = tenant.TenantName ?? string.Empty,
        ExamName = x.Exam.Name,
        TermName = x.Exam.ExamTerm?.Name,
        ClassName = x.Class.Name,
        SectionName = x.Section.Name,
        SubjectCount = x.Subjects.Count
    };

    private static ExamScheduleDetailDto MapDetail(ExamSchedule x) => new()
    {
        Id = x.Id,
        ExamName = x.Exam.Name,
        TermName = x.Exam.ExamTerm?.Name ?? string.Empty,
        ClassName = x.Class.Name,
        SectionName = x.Section.Name,
        Subjects = x.Subjects.OrderBy(s => s.SortOrder).Select(s => new ExamScheduleSubjectDetailDto
        {
            SubjectName = s.Subject.Name,
            ExamDate = s.ExamDate.ToString("dd/MMM/yyyy", DateCulture),
            StartingTime = DateTime.Today.Add(s.StartingTime).ToString("h:mm tt", DateCulture),
            EndingTime = DateTime.Today.Add(s.EndingTime).ToString("h:mm tt", DateCulture),
            HallRoom = s.Hall?.HallNo
        }).ToList()
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

    private bool IsStudent() => Roles().Contains(AppConstants.Roles.Student);

    private void Manage()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin))
            throw new ForbiddenException("Only Super Admin or School Admin can manage exam schedules.");
    }

    private void Read()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin) &&
            !r.Contains(AppConstants.Roles.Teacher) && !r.Contains(AppConstants.Roles.Student))
            throw new ForbiddenException("You do not have access to exam schedules.");
    }
}
