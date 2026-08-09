using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.StudentAccounting;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Helpers;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class OfflinePaymentService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http) : IOfflinePaymentService
{
    public async Task<OfflinePaymentResponseDto> SubmitAsync(CreateOfflinePaymentDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        var student = await ResolveSubmitStudentAsync(dto.StudentId, ct);

        if (dto.Amount <= 0)
            throw new AppException("Amount must be greater than zero.", 400);

        if (dto.PaymentTypeId.HasValue)
        {
            var pt = await uow.OfflinePaymentTypes.GetByIdAsync(dto.PaymentTypeId.Value, ct)
                ?? throw new NotFoundException($"Payment type '{dto.PaymentTypeId}' not found.");
            if (!pt.IsActive)
                throw new AppException($"Payment type '{pt.Name}' is inactive.", 400);
        }

        var trxId = await GenerateUniqueTrxIdAsync(ct);
        var payment = new OfflinePayment
        {
            Id = Guid.NewGuid(),
            TrxId = trxId,
            StudentId = student.Id,
            PaymentTypeId = dto.PaymentTypeId,
            ClassId = student.ClassId,
            SectionId = student.SectionId,
            PaymentDate = DateTime.SpecifyKind(dto.PaymentDate == default ? DateTime.UtcNow.Date : dto.PaymentDate.Date, DateTimeKind.Utc),
            SubmitDate = DateTime.UtcNow,
            Amount = dto.Amount,
            Status = OfflinePaymentStatuses.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await uow.OfflinePayments.AddAsync(payment, ct);
        await uow.SaveTenantChangesAsync(ct);
        return Map(await uow.OfflinePayments.GetByIdAsync(payment.Id, ct) ?? payment);
    }

    public async Task<OfflinePaymentListResponseDto> GetFilteredAsync(OfflinePaymentFilterDto filter, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var page = filter.Page < 1 ? 1 : filter.Page;
        var size = filter.PageSize is < 1 or > 200 ? 25 : filter.PageSize;
        var (items, total) = await uow.OfflinePayments.GetFilteredAsync(new OfflinePaymentFilter
        {
            Status = filter.Status,
            Page = page,
            PageSize = size
        }, ct);

        return new OfflinePaymentListResponseDto
        {
            Data = items.Select(Map).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = size,
            TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)size)
        };
    }

    public async Task<IReadOnlyList<OfflinePaymentResponseDto>> GetMyPaymentsAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        var userId = CurrentUser();
        var roles = Roles();
        var studentIds = new List<Guid>();

        if (roles.Contains(AppConstants.Roles.Parent) && !roles.Contains(AppConstants.Roles.Admin) && !roles.Contains(AppConstants.Roles.SuperAdmin))
        {
            var wards = await uow.Students.GetByGuardianUserIdAsync(userId, ct);
            studentIds.AddRange(wards.Select(w => w.Id));
        }
        else
        {
            var student = await uow.Students.GetByUserIdAsync(userId, ct)
                ?? throw new NotFoundException("No student profile found for current user.");
            studentIds.Add(student.Id);
        }

        var result = new List<OfflinePaymentResponseDto>();
        foreach (var id in studentIds)
        {
            var (items, _) = await uow.OfflinePayments.GetFilteredAsync(new OfflinePaymentFilter { StudentId = id, PageSize = 500 }, ct);
            result.AddRange(items.Select(Map));
        }
        return result.OrderByDescending(x => x.SubmitDate).ToList();
    }

    public async Task<OfflinePaymentResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        var payment = await uow.OfflinePayments.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Offline payment '{id}' not found.");
        await EnsureManageOrOwnerAsync(payment, ct);
        return Map(payment);
    }

    public async Task<OfflinePaymentResponseDto> ApproveAsync(Guid id, ReviewOfflinePaymentDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var payment = await RequirePending(id, ct);
        payment.Status = OfflinePaymentStatuses.Approved;
        payment.UpdatedAt = DateTime.UtcNow;
        await uow.OfflinePayments.UpdateAsync(payment, ct);

        await CreditOldestUnpaidInvoiceAsync(payment.StudentId, payment.Amount, ct);

        await uow.SaveTenantChangesAsync(ct);
        return Map(await uow.OfflinePayments.GetByIdAsync(id, ct) ?? payment);
    }

    public async Task<OfflinePaymentResponseDto> RejectAsync(Guid id, ReviewOfflinePaymentDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var payment = await RequirePending(id, ct);
        payment.Status = OfflinePaymentStatuses.Rejected;
        payment.UpdatedAt = DateTime.UtcNow;
        await uow.OfflinePayments.UpdateAsync(payment, ct);
        await uow.SaveTenantChangesAsync(ct);
        return Map(await uow.OfflinePayments.GetByIdAsync(id, ct) ?? payment);
    }

    private async Task CreditOldestUnpaidInvoiceAsync(Guid studentId, decimal amount, CancellationToken ct)
    {
        var invoices = await uow.StudentFeeInvoices.GetByStudentAsync(studentId, ct);
        var target = invoices
            .Where(x => x.Status != FeeInvoiceStatuses.Paid)
            .OrderBy(x => x.CreatedAt)
            .FirstOrDefault();
        if (target is null) return;

        target.PaidAmount += amount;
        target.DueAmount = target.TotalAmount + target.FineAmount - target.PaidAmount;
        target.Status = target.DueAmount <= 0
            ? FeeInvoiceStatuses.Paid
            : (target.PaidAmount > 0 ? FeeInvoiceStatuses.Partial : FeeInvoiceStatuses.Unpaid);
        if (target.DueAmount < 0) target.DueAmount = 0;
        target.UpdatedAt = DateTime.UtcNow;
        await uow.StudentFeeInvoices.UpdateAsync(target, ct);
    }

    private async Task<OfflinePayment> RequirePending(Guid id, CancellationToken ct)
    {
        var payment = await uow.OfflinePayments.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Offline payment '{id}' not found.");
        if (payment.Status != OfflinePaymentStatuses.Pending)
            throw new AppException($"Cannot review a payment that is already {payment.Status}.", 400);
        return payment;
    }

    private async Task<Student> ResolveSubmitStudentAsync(Guid? requestedStudentId, CancellationToken ct)
    {
        var userId = CurrentUser();
        var roles = Roles();

        if (roles.Contains(AppConstants.Roles.Admin) || roles.Contains(AppConstants.Roles.SuperAdmin) || roles.Contains(AppConstants.Roles.Accountant))
        {
            if (!requestedStudentId.HasValue)
                throw new AppException("StudentId is required.", 400);
            return await uow.Students.GetByIdAsync(requestedStudentId.Value, ct)
                ?? throw new NotFoundException($"Student '{requestedStudentId}' not found.");
        }

        if (roles.Contains(AppConstants.Roles.Parent))
        {
            var wards = await uow.Students.GetByGuardianUserIdAsync(userId, ct);
            if (wards.Count == 0)
                throw new NotFoundException("No student profile found for current user.");
            if (requestedStudentId.HasValue)
                return wards.FirstOrDefault(w => w.Id == requestedStudentId.Value)
                    ?? throw new ForbiddenException("You can only submit payments for your own children.");
            return wards[0];
        }

        return await uow.Students.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException("No student profile found for current user.");
    }

    private async Task EnsureManageOrOwnerAsync(OfflinePayment payment, CancellationToken ct)
    {
        var r = Roles();
        if (r.Contains(AppConstants.Roles.Admin) || r.Contains(AppConstants.Roles.SuperAdmin) || r.Contains(AppConstants.Roles.Accountant))
            return;

        var userId = CurrentUser();
        if (r.Contains(AppConstants.Roles.Parent))
        {
            var wards = await uow.Students.GetByGuardianUserIdAsync(userId, ct);
            if (wards.Any(w => w.Id == payment.StudentId)) return;
            throw new ForbiddenException("You do not have access to this payment.");
        }

        var student = await uow.Students.GetByUserIdAsync(userId, ct);
        if (student is not null && student.Id == payment.StudentId) return;
        throw new ForbiddenException("You do not have access to this payment.");
    }

    private async Task<string> GenerateUniqueTrxIdAsync(CancellationToken ct)
    {
        for (var i = 0; i < 5; i++)
        {
            var trxId = AccountingHelpers.GenerateTrxId();
            if (!await uow.OfflinePayments.TrxIdExistsAsync(trxId, ct))
                return trxId;
        }
        throw new AppException("Could not generate a unique transaction id. Please try again.", 500);
    }

    private static OfflinePaymentResponseDto Map(OfflinePayment x) => new()
    {
        Id = x.Id,
        TrxId = x.TrxId,
        StudentId = x.StudentId,
        StudentName = StudentName(x.Student),
        RegisterNo = x.Student?.RegisterNo ?? string.Empty,
        PaymentTypeId = x.PaymentTypeId,
        PaymentTypeName = x.PaymentType?.Name,
        ClassName = x.Class?.Name,
        SectionName = x.Section?.Name,
        PaymentDate = x.PaymentDate,
        SubmitDate = x.SubmitDate,
        Amount = x.Amount,
        Status = x.Status
    };

    private static string StudentName(Student? s)
        => s is null ? string.Empty : (string.IsNullOrWhiteSpace(s.LastName) ? s.FirstName.Trim() : $"{s.FirstName.Trim()} {s.LastName.Trim()}");

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
            throw new ForbiddenException("Only Super Admin, School Admin, or Accountant can manage offline payments.");
    }

    private Guid CurrentUser()
    {
        var c = http.HttpContext?.User.FindFirst(AppConstants.Claims.UserId)
            ?? http.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        if (c is null || !Guid.TryParse(c.Value, out var id)) throw new UnauthorizedException();
        return id;
    }
}
