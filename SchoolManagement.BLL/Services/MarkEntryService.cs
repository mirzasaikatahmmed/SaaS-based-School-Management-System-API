using System.Security.Claims;
using System.Text;
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

public class MarkEntryService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http) : IMarkEntryService
{
    public async Task<MarkEntryListResponseDto> GetListAsync(MarkEntryFilterDto filter, CancellationToken ct = default)
    {
        await Ready(ct);
        await ValidateAccessForRead(filter, ct);

        var exam = await uow.Exams.GetByIdAsync(filter.ExamId, ct)
            ?? throw new NotFoundException($"Exam '{filter.ExamId}' not found.");
        var scheduleSubject = await uow.MarkEntries.GetScheduleSubjectAsync(
            filter.ExamId, filter.ClassId, filter.SectionId, filter.SubjectId, ct);

        var hasMcq = exam.MarkDistributions.Any(d =>
            d.MarkDistribution.Name.Equals("MCQ", StringComparison.OrdinalIgnoreCase));

        var students = await uow.MarkEntries.GetActiveStudentsByClassSectionAsync(filter.ClassId, filter.SectionId, ct);
        if (IsStudent())
        {
            var current = await GetCurrentStudentAsync(ct);
            students = students.Where(s => s.Id == current.Id).ToList();
        }

        var existing = await uow.MarkEntries.GetForFilterAsync(
            filter.ExamId, filter.ClassId, filter.SectionId, filter.SubjectId, ct);
        var byStudent = existing.ToDictionary(m => m.StudentId);

        var items = students.Select((s, i) =>
        {
            byStudent.TryGetValue(s.Id, out var mark);
            return new MarkEntryStudentItemDto
            {
                StudentId = s.Id,
                MarkEntryId = mark?.Id,
                Sl = i + 1,
                StudentName = StudentName(s),
                Category = s.Category?.Name,
                RegisterNo = s.RegisterNo,
                Roll = s.Roll,
                IsAbsent = mark?.IsAbsent ?? false,
                WrittenMark = mark?.WrittenMark,
                McqMark = mark?.McqMark,
                WrittenFullMark = scheduleSubject?.WrittenFullMark,
                WrittenPassMark = scheduleSubject?.WrittenPassMark
            };
        }).ToList();

        return new MarkEntryListResponseDto
        {
            HasMcq = hasMcq,
            WrittenFullMark = scheduleSubject?.WrittenFullMark,
            WrittenPassMark = scheduleSubject?.WrittenPassMark,
            Items = items
        };
    }

    public async Task SaveAsync(SaveMarkEntriesDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        ManageOrTeacher();

        var exam = await uow.Exams.GetByIdAsync(dto.ExamId, ct)
            ?? throw new NotFoundException($"Exam '{dto.ExamId}' not found.");
        var scheduleSubject = await uow.MarkEntries.GetScheduleSubjectAsync(
            dto.ExamId, dto.ClassId, dto.SectionId, dto.SubjectId, ct)
            ?? throw new AppException("Exam schedule subject not found for the selected filters.", 400);

        var hasMcq = exam.MarkDistributions.Any(d =>
            d.MarkDistribution.Name.Equals("MCQ", StringComparison.OrdinalIgnoreCase));
        var fullMark = scheduleSubject.WrittenFullMark;

        var entries = new List<MarkEntry>();
        foreach (var mark in dto.Marks)
        {
            decimal? written = mark.IsAbsent ? null : mark.WrittenMark;
            decimal? mcq = mark.IsAbsent ? null : mark.McqMark;

            if (!mark.IsAbsent && fullMark.HasValue && written.HasValue &&
                (written.Value < 0 || written.Value > fullMark.Value))
                throw new AppException($"Written mark must be between 0 and {fullMark.Value}.", 400);

            if (!mark.IsAbsent && mcq.HasValue && mcq.Value < 0)
                throw new AppException("MCQ mark cannot be negative.", 400);

            decimal? total = mark.IsAbsent ? null : (written ?? 0) + (hasMcq ? mcq ?? 0 : 0);

            entries.Add(new MarkEntry
            {
                Id = Guid.NewGuid(),
                ExamId = dto.ExamId,
                ClassId = dto.ClassId,
                SectionId = dto.SectionId,
                SubjectId = dto.SubjectId,
                StudentId = mark.StudentId,
                IsAbsent = mark.IsAbsent,
                WrittenMark = written,
                McqMark = hasMcq ? mcq : null,
                TotalMark = total,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await uow.BeginTenantTransactionAsync(ct);
        try
        {
            await uow.MarkEntries.UpsertBatchAsync(entries, ct);
            await uow.SaveTenantChangesAsync(ct);
            await uow.CommitTenantTransactionAsync(ct);
        }
        catch
        {
            await uow.RollbackTenantTransactionAsync(ct);
            throw;
        }
    }

    public async Task<(byte[] Content, string ContentType, string FileName)> ExportAsync(MarkEntryFilterDto filter, CancellationToken ct = default)
    {
        ManageOrTeacher();
        var list = await GetListAsync(filter, ct);
        var header = list.HasMcq
            ? "Branch,RegisterNo,StudentName,Roll,Category,IsAbsent,WrittenMark,McqMark,TotalMark\n"
            : "Branch,RegisterNo,StudentName,Roll,Category,IsAbsent,WrittenMark,TotalMark\n";

        var sb = new StringBuilder(header);
        var branch = tenant.TenantName ?? string.Empty;
        foreach (var x in list.Items)
        {
            var total = x.IsAbsent ? "" : ((x.WrittenMark ?? 0) + (list.HasMcq ? x.McqMark ?? 0 : 0)).ToString();
            if (list.HasMcq)
                sb.AppendLine($"{Csv(branch)},{Csv(x.RegisterNo)},{Csv(x.StudentName)},{Csv(x.Roll)},{Csv(x.Category)},{x.IsAbsent},{x.WrittenMark},{x.McqMark},{total}");
            else
                sb.AppendLine($"{Csv(branch)},{Csv(x.RegisterNo)},{Csv(x.StudentName)},{Csv(x.Roll)},{Csv(x.Category)},{x.IsAbsent},{x.WrittenMark},{total}");
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        var fmt = (filter.Export ?? "csv").ToLowerInvariant();
        return fmt switch
        {
            "csv" => (bytes, "text/csv", $"mark-entries-{DateTime.UtcNow:yyyyMMdd}.csv"),
            "excel" => (bytes, "application/vnd.ms-excel", $"mark-entries-{DateTime.UtcNow:yyyyMMdd}.xls"),
            _ => throw new AppException("Unsupported export format. Use csv or excel.", 400)
        };
    }

    private async Task ValidateAccessForRead(MarkEntryFilterDto filter, CancellationToken ct)
    {
        var exam = await uow.Exams.GetByIdAsync(filter.ExamId, ct)
            ?? throw new NotFoundException($"Exam '{filter.ExamId}' not found.");

        if (IsStudent())
        {
            if (!exam.IsResultPublished)
                throw new ForbiddenException("Exam results are not published.");
            return;
        }

        var r = Roles();
        if (r.Contains(AppConstants.Roles.Admin) || r.Contains(AppConstants.Roles.SuperAdmin) ||
            r.Contains(AppConstants.Roles.Teacher))
            return;

        throw new ForbiddenException("You do not have access to mark entries.");
    }

    private async Task<Student> GetCurrentStudentAsync(CancellationToken ct)
    {
        var userId = CurrentUser();
        return await uow.Students.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException("No student profile found for current user.");
    }

    private static string StudentName(Student s)
        => string.IsNullOrWhiteSpace(s.LastName) ? s.FirstName.Trim() : $"{s.FirstName.Trim()} {s.LastName.Trim()}";

    private static string Csv(string? v) => string.IsNullOrEmpty(v) ? "" : $"\"{v.Replace("\"", "\"\"")}\"";

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

    private void ManageOrTeacher()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin) &&
            !r.Contains(AppConstants.Roles.Teacher))
            throw new ForbiddenException("Only Super Admin, School Admin, or Teacher can manage mark entries.");
    }

    private Guid CurrentUser()
    {
        var c = http.HttpContext?.User.FindFirst(AppConstants.Claims.UserId)
            ?? http.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        if (c is null || !Guid.TryParse(c.Value, out var id)) throw new UnauthorizedException();
        return id;
    }
}
