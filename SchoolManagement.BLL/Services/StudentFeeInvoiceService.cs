using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.StudentAccounting;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class StudentFeeInvoiceService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IFeesAllocationService allocationService,
    IHttpContextAccessor http) : IStudentFeeInvoiceService
{
    public async Task<StudentFeeInvoiceListResponseDto> GetFilteredAsync(StudentFeeInvoiceFilterDto filter, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        return await Search(new StudentFeeInvoiceFilter
        {
            ClassId = filter.ClassId,
            SectionId = filter.SectionId,
            Status = filter.Status,
            Page = filter.Page,
            PageSize = filter.PageSize
        }, ct);
    }

    public async Task<StudentFeeInvoiceListResponseDto> GetDueAsync(DueInvoiceFilterDto filter, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var page = filter.Page < 1 ? 1 : filter.Page;
        var size = filter.PageSize is < 1 or > 200 ? 25 : filter.PageSize;
        var (items, total) = await uow.StudentFeeInvoices.GetDueAsync(new DueInvoiceFilter
        {
            ClassId = filter.ClassId,
            SectionId = filter.SectionId,
            FeesTypeId = filter.FeesTypeId,
            OverdueOnly = filter.OverdueOnly,
            Page = page,
            PageSize = size
        }, ct);

        return new StudentFeeInvoiceListResponseDto
        {
            Data = items.Select((x, i) => Map(x, (page - 1) * size + i + 1)).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = size,
            TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)size)
        };
    }

    public async Task<IReadOnlyList<StudentFeeInvoiceResponseDto>> GetMyInvoicesAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        var userId = CurrentUser();
        var roles = Roles();

        if (roles.Contains(AppConstants.Roles.Parent) && !roles.Contains(AppConstants.Roles.Admin) && !roles.Contains(AppConstants.Roles.SuperAdmin))
        {
            var wards = await uow.Students.GetByGuardianUserIdAsync(userId, ct);
            var result = new List<StudentFeeInvoiceResponseDto>();
            foreach (var w in wards)
                result.AddRange((await uow.StudentFeeInvoices.GetByStudentAsync(w.Id, ct)).Select((x, i) => Map(x, i + 1)));
            return result;
        }

        var student = await uow.Students.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException("No student profile found for current user.");
        return (await uow.StudentFeeInvoices.GetByStudentAsync(student.Id, ct)).Select((x, i) => Map(x, i + 1)).ToList();
    }

    public async Task<IReadOnlyList<StudentFeeInvoiceResponseDto>> GetByStudentAsync(Guid studentId, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        return (await uow.StudentFeeInvoices.GetByStudentAsync(studentId, ct)).Select((x, i) => Map(x, i + 1)).ToList();
    }

    public async Task<StudentFeeInvoiceResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        var invoice = await uow.StudentFeeInvoices.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Invoice '{id}' not found.");
        await EnsureManageOrOwnerAsync(invoice, ct);
        return Map(invoice, 0);
    }

    public async Task<StudentFeeInvoiceResponseDto> PayAsync(Guid id, PayInvoiceDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        if (dto.Amount <= 0)
            throw new AppException("Amount must be greater than zero.", 400);

        var invoice = await uow.StudentFeeInvoices.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Invoice '{id}' not found.");
        if (invoice.Status == FeeInvoiceStatuses.Paid)
            throw new AppException("This invoice is already fully paid.", 400);

        ApplyPayment(invoice, dto.Amount);
        await uow.StudentFeeInvoices.UpdateAsync(invoice, ct);
        await uow.SaveTenantChangesAsync(ct);
        return Map(await uow.StudentFeeInvoices.GetByIdAsync(id, ct) ?? invoice, 0);
    }

    public async Task<GenerateInvoicesResultDto> GenerateAsync(GenerateInvoicesDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();

        if (dto.FeesAllocationId.HasValue)
            return await allocationService.GenerateInvoicesForAllocationAsync(dto.FeesAllocationId.Value, ct);

        var allocations = await uow.FeesAllocations.GetFilteredAsync(new FeesAllocationFilter
        {
            ClassId = dto.ClassId,
            SectionId = dto.SectionId,
            IsActive = true
        }, ct);

        if (allocations.Count == 0)
            throw new NotFoundException("No active fees allocation found for the given class and section.");

        var total = new GenerateInvoicesResultDto();
        foreach (var a in allocations)
        {
            var r = await allocationService.GenerateInvoicesForAllocationAsync(a.Id, ct);
            total.Generated += r.Generated;
            total.Skipped += r.Skipped;
            total.TotalStudents += r.TotalStudents;
        }
        return total;
    }

    public async Task<(byte[] Content, string ContentType, string FileName)> ExportAsync(StudentFeeInvoiceFilterDto filter, CancellationToken ct = default)
    {
        var list = await GetFilteredAsync(new StudentFeeInvoiceFilterDto
        {
            ClassId = filter.ClassId,
            SectionId = filter.SectionId,
            Status = filter.Status,
            Page = 1,
            PageSize = 5000
        }, ct);

        var sb = new StringBuilder("Sl,Branch,RegisterNo,StudentName,Class,Section,FeesGroup,TotalAmount,FineAmount,PaidAmount,DueAmount,Status,CreatedAt\n");
        foreach (var x in list.Data)
            sb.AppendLine($"{x.Sl},{Csv(x.Branch)},{Csv(x.RegisterNo)},{Csv(x.StudentName)},{Csv(x.ClassName)},{Csv(x.SectionName)},{Csv(x.FeesGroupName)},{x.TotalAmount},{x.FineAmount},{x.PaidAmount},{x.DueAmount},{Csv(x.Status)},{x.CreatedAt:yyyy-MM-dd}");

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return (bytes, "text/csv", "invoices.csv");
    }

    private static void ApplyPayment(StudentFeeInvoice invoice, decimal amount)
    {
        invoice.PaidAmount += amount;
        invoice.DueAmount = invoice.TotalAmount + invoice.FineAmount - invoice.PaidAmount;
        invoice.Status = invoice.DueAmount <= 0
            ? FeeInvoiceStatuses.Paid
            : (invoice.PaidAmount > 0 ? FeeInvoiceStatuses.Partial : FeeInvoiceStatuses.Unpaid);
        if (invoice.DueAmount < 0) invoice.DueAmount = 0;
        invoice.UpdatedAt = DateTime.UtcNow;
    }

    private async Task<StudentFeeInvoiceListResponseDto> Search(StudentFeeInvoiceFilter filter, CancellationToken ct)
    {
        var page = filter.Page < 1 ? 1 : filter.Page;
        var size = filter.PageSize is < 1 or > 200 ? 25 : filter.PageSize;
        filter.Page = page;
        filter.PageSize = size;
        var (items, total) = await uow.StudentFeeInvoices.GetFilteredAsync(filter, ct);
        return new StudentFeeInvoiceListResponseDto
        {
            Data = items.Select((x, i) => Map(x, (page - 1) * size + i + 1)).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = size,
            TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)size)
        };
    }

    private StudentFeeInvoiceResponseDto Map(StudentFeeInvoice x, int sl) => new()
    {
        Id = x.Id,
        Sl = sl,
        StudentId = x.StudentId,
        StudentName = StudentName(x.Student),
        RegisterNo = x.Student?.RegisterNo ?? string.Empty,
        ClassId = x.ClassId,
        ClassName = x.Class?.Name ?? string.Empty,
        SectionId = x.SectionId,
        SectionName = x.Section?.Name ?? string.Empty,
        FeesGroupId = x.FeesGroupId,
        FeesGroupName = x.FeesGroup?.Name ?? string.Empty,
        FeesAllocationId = x.FeesAllocationId,
        TotalAmount = x.TotalAmount,
        PaidAmount = x.PaidAmount,
        FineAmount = x.FineAmount,
        DueAmount = x.DueAmount,
        Status = x.Status,
        Branch = tenant.TenantName ?? string.Empty,
        CreatedAt = x.CreatedAt
    };

    private static string StudentName(Student? s)
        => s is null ? string.Empty : (string.IsNullOrWhiteSpace(s.LastName) ? s.FirstName.Trim() : $"{s.FirstName.Trim()} {s.LastName.Trim()}");

    private async Task EnsureManageOrOwnerAsync(StudentFeeInvoice invoice, CancellationToken ct)
    {
        var r = Roles();
        if (r.Contains(AppConstants.Roles.Admin) || r.Contains(AppConstants.Roles.SuperAdmin) || r.Contains(AppConstants.Roles.Accountant))
            return;

        var userId = CurrentUser();
        if (r.Contains(AppConstants.Roles.Parent))
        {
            var wards = await uow.Students.GetByGuardianUserIdAsync(userId, ct);
            if (wards.Any(w => w.Id == invoice.StudentId)) return;
            throw new ForbiddenException("You do not have access to this invoice.");
        }

        var student = await uow.Students.GetByUserIdAsync(userId, ct);
        if (student is not null && student.Id == invoice.StudentId) return;
        throw new ForbiddenException("You do not have access to this invoice.");
    }

    private async Task Ready(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(tenant.SchemaName))
            throw new AppException("X-Tenant-ID header is required.", 400);
        await provisioner.EnsureStudentAndOfficeAccountingModuleAsync(tenant.SchemaName!, ct);
    }

    private HashSet<string> Roles()
    {
        var p = http.HttpContext?.User;
        if (p is null) return [];
        return p.FindAll("role").Concat(p.FindAll(ClaimTypes.Role)).Select(x => x.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private void Manage()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin) && !r.Contains(AppConstants.Roles.Accountant))
            throw new ForbiddenException("Only Super Admin, School Admin, or Accountant can manage student fee invoices.");
    }

    private Guid CurrentUser()
    {
        var c = http.HttpContext?.User.FindFirst(AppConstants.Claims.UserId)
            ?? http.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        if (c is null || !Guid.TryParse(c.Value, out var id)) throw new UnauthorizedException();
        return id;
    }

    private static string Csv(string? v) => string.IsNullOrEmpty(v) ? "" : $"\"{v.Replace("\"", "\"\"")}\"";
}
