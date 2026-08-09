using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.AdvanceSalary;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class AdvanceSalaryService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IStorageService storage,
    IHttpContextAccessor http) : IAdvanceSalaryService
{
    private static readonly Regex MonthRegex = new(@"^\d{4}-(0[1-9]|1[0-2])$", RegexOptions.Compiled);

    public async Task<AdvanceSalaryMyListResponseDto> GetMyListAsync(AdvanceSalaryFilterDto filter, CancellationToken ct = default)
    {
        await Ready(ct);
        var employee = await CurrentEmployee(ct);
        var page = filter.Page < 1 ? 1 : filter.Page;
        var size = filter.PageSize is < 1 or > 200 ? 25 : filter.PageSize;
        var (items, total) = await uow.AdvanceSalaries.SearchAsync(new AdvanceSalarySearchFilter
        {
            EmployeeId = employee.Id,
            DeductMonth = filter.DeductMonth,
            Search = filter.Search,
            Page = page,
            PageSize = size
        }, ct);

        var data = new List<AdvanceSalaryMyListItemDto>();
        for (var i = 0; i < items.Count; i++)
        {
            var x = items[i];
            data.Add(new AdvanceSalaryMyListItemDto
            {
                Id = x.Id,
                Sl = (page - 1) * size + i + 1,
                PhotoUrl = await Presign(x.Employee.ProfilePictureUrl, ct),
                ApplicantName = x.Employee.Name,
                DeductMonth = x.DeductMonth,
                Amount = x.Amount,
                AppliedOn = x.AppliedOn,
                CreatedAt = x.CreatedAt,
                Status = x.Status
            });
        }

        return new AdvanceSalaryMyListResponseDto
        {
            Data = data,
            TotalCount = total,
            Page = page,
            PageSize = size,
            TotalPages = Pages(total, size)
        };
    }

    public async Task<AdvanceSalaryResponseDto> CreateMyAsync(CreateMyAdvanceSalaryDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        ValidateMonth(dto.DeductMonth);
        if (dto.Amount <= 0) throw new AppException("Amount must be greater than zero.", 400);
        var employee = await CurrentEmployee(ct);
        return await CreateInternal(employee.Id, dto.DeductMonth, dto.Amount, dto.Reason, ct);
    }

    public async Task<AdvanceSalaryListResponseDto> GetManageListAsync(AdvanceSalaryManageFilterDto filter, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var page = filter.Page < 1 ? 1 : filter.Page;
        var size = filter.PageSize is < 1 or > 200 ? 25 : filter.PageSize;
        var (items, total) = await uow.AdvanceSalaries.SearchAsync(new AdvanceSalarySearchFilter
        {
            DeductMonth = filter.DeductMonth,
            Status = filter.Status,
            Search = filter.Search,
            Page = page,
            PageSize = size
        }, ct);

        var data = new List<AdvanceSalaryListItemDto>();
        for (var i = 0; i < items.Count; i++)
            data.Add(await MapList(items[i], (page - 1) * size + i + 1, ct));

        return new AdvanceSalaryListResponseDto
        {
            Data = data,
            TotalCount = total,
            Page = page,
            PageSize = size,
            TotalPages = Pages(total, size)
        };
    }

    public async Task<AdvanceSalaryResponseDto> CreateForEmployeeAsync(CreateAdvanceSalaryDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        ValidateMonth(dto.DeductMonth);
        if (dto.Amount <= 0) throw new AppException("Amount must be greater than zero.", 400);
        var employee = await uow.Employees.GetByIdWithDetailsAsync(dto.EmployeeId, ct)
            ?? throw new NotFoundException($"Employee '{dto.EmployeeId}' not found.");
        return await CreateInternal(employee.Id, dto.DeductMonth, dto.Amount, dto.Reason, ct);
    }

    public async Task<AdvanceSalaryResponseDto> ApproveAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var x = await uow.AdvanceSalaries.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Advance salary request '{id}' not found.");
        EnsurePending(x);
        x.Status = HrRequestStatuses.Approved;
        x.ReviewedBy = CurrentUserOrNull();
        x.ReviewedAt = DateTime.UtcNow;
        x.RejectReason = null;
        x.UpdatedAt = DateTime.UtcNow;
        await uow.AdvanceSalaries.UpdateAsync(x, ct);
        await uow.SaveTenantChangesAsync(ct);
        return await MapDetail(await uow.AdvanceSalaries.GetByIdAsync(id, ct) ?? x, ct);
    }

    public async Task<AdvanceSalaryResponseDto> RejectAsync(Guid id, ReviewAdvanceSalaryDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        if (string.IsNullOrWhiteSpace(dto.RejectReason))
            throw new AppException("RejectReason is required.", 400);
        var x = await uow.AdvanceSalaries.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Advance salary request '{id}' not found.");
        EnsurePending(x);
        x.Status = HrRequestStatuses.Rejected;
        x.RejectReason = dto.RejectReason.Trim();
        x.ReviewedBy = CurrentUserOrNull();
        x.ReviewedAt = DateTime.UtcNow;
        x.UpdatedAt = DateTime.UtcNow;
        await uow.AdvanceSalaries.UpdateAsync(x, ct);
        await uow.SaveTenantChangesAsync(ct);
        return await MapDetail(await uow.AdvanceSalaries.GetByIdAsync(id, ct) ?? x, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var x = await uow.AdvanceSalaries.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Advance salary request '{id}' not found.");
        if (x.Status == HrRequestStatuses.Approved)
            throw new AppException("Cannot delete an approved advance salary", 400);
        await uow.AdvanceSalaries.DeleteAsync(x, ct);
        await uow.SaveTenantChangesAsync(ct);
    }

    public async Task<(byte[] Content, string ContentType, string FileName)> ExportAsync(AdvanceSalaryManageFilterDto filter, CancellationToken ct = default)
    {
        var list = await GetManageListAsync(new AdvanceSalaryManageFilterDto
        {
            DeductMonth = filter.DeductMonth,
            Status = filter.Status,
            Search = filter.Search,
            Page = 1,
            PageSize = 500
        }, ct);

        var sb = new StringBuilder("StaffId,ApplicantName,Role,DeductMonth,Amount,Status,AppliedOn\n");
        foreach (var x in list.Data)
            sb.AppendLine($"{Csv(x.StaffId)},{Csv(x.ApplicantName)},{Csv(x.StaffRole)},{Csv(x.DeductMonth)},{x.Amount},{Csv(x.Status)},{x.AppliedOn:O}");

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        var fmt = (filter.Export ?? "csv").ToLowerInvariant();
        return fmt switch
        {
            "csv" => (bytes, "text/csv", $"advance-salary-{DateTime.UtcNow:yyyyMMdd}.csv"),
            "excel" => (bytes, "application/vnd.ms-excel", $"advance-salary-{DateTime.UtcNow:yyyyMMdd}.xls"),
            "pdf" => (bytes, "application/pdf", $"advance-salary-{DateTime.UtcNow:yyyyMMdd}.pdf"),
            _ => throw new AppException("Unsupported export format. Use csv, excel, or pdf.", 400)
        };
    }

    public async Task<IReadOnlyList<HrEmployeeLookupDto>> GetEmployeeLookupAsync(string role, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        if (string.IsNullOrWhiteSpace(role)) throw new AppException("Role is required.", 400);
        var (items, _) = await uow.Employees.SearchAsync(new EmployeeSearchFilter
        {
            Role = role,
            IsActive = true,
            Page = 1,
            PageSize = 200
        }, ct);
        return items.Select(e => new HrEmployeeLookupDto { Id = e.Id, Name = e.Name, StaffId = e.StaffId }).ToList();
    }

    private async Task<AdvanceSalaryResponseDto> CreateInternal(Guid employeeId, string month, decimal amount, string? reason, CancellationToken ct)
    {
        if (await uow.AdvanceSalaries.HasPendingForMonthAsync(employeeId, month, ct))
            throw new AppException("You already have a pending request for this month", 400);

        var entity = new AdvanceSalaryRequest
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            DeductMonth = month,
            Amount = amount,
            Reason = reason,
            Status = HrRequestStatuses.Pending,
            AppliedOn = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await uow.AdvanceSalaries.AddAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
        return await MapDetail(await uow.AdvanceSalaries.GetByIdAsync(entity.Id, ct) ?? entity, ct);
    }

    private static void EnsurePending(AdvanceSalaryRequest x)
    {
        if (x.Status != HrRequestStatuses.Pending)
            throw new AppException($"Cannot review a request that is already {x.Status}.", 400);
    }

    private async Task<AdvanceSalaryListItemDto> MapList(AdvanceSalaryRequest x, int sl, CancellationToken ct) => new()
    {
        Id = x.Id,
        Sl = sl,
        PhotoUrl = await Presign(x.Employee?.ProfilePictureUrl, ct),
        Branch = tenant.TenantName ?? string.Empty,
        ApplicantName = x.Employee?.Name ?? string.Empty,
        StaffId = x.Employee?.StaffId ?? string.Empty,
        StaffRole = x.Employee?.Role ?? string.Empty,
        DeductMonth = x.DeductMonth,
        Amount = x.Amount,
        AppliedOn = x.AppliedOn,
        CreatedAt = x.CreatedAt,
        Status = x.Status,
        RejectReason = x.RejectReason
    };

    private async Task<AdvanceSalaryResponseDto> MapDetail(AdvanceSalaryRequest x, CancellationToken ct)
    {
        var list = await MapList(x, 0, ct);
        return new AdvanceSalaryResponseDto
        {
            Id = list.Id,
            Sl = list.Sl,
            PhotoUrl = list.PhotoUrl,
            Branch = list.Branch,
            ApplicantName = list.ApplicantName,
            StaffId = list.StaffId,
            StaffRole = list.StaffRole,
            DeductMonth = list.DeductMonth,
            Amount = list.Amount,
            AppliedOn = list.AppliedOn,
            CreatedAt = list.CreatedAt,
            Status = list.Status,
            RejectReason = list.RejectReason,
            Reason = x.Reason,
            ReviewedAt = x.ReviewedAt
        };
    }

    private static void ValidateMonth(string? month)
    {
        if (string.IsNullOrWhiteSpace(month) || !MonthRegex.IsMatch(month))
            throw new AppException("DeductMonth must be in YYYY-MM format.", 400);
    }

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
        return p.FindAll("role").Concat(p.FindAll(ClaimTypes.Role)).Select(x => x.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private void Manage()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) &&
            !r.Contains(AppConstants.Roles.SuperAdmin) &&
            !r.Contains(AppConstants.Roles.Accountant))
            throw new ForbiddenException("Only Super Admin, School Admin, or Accountant can manage advance salary.");
    }

    private async Task<Employee> CurrentEmployee(CancellationToken ct)
    {
        var userId = CurrentUser();
        return await uow.Employees.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException("Employee profile not found for current user.");
    }

    private Guid CurrentUser()
    {
        var c = http.HttpContext?.User.FindFirst(AppConstants.Claims.UserId)
            ?? http.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        if (c is null || !Guid.TryParse(c.Value, out var id)) throw new UnauthorizedException();
        return id;
    }

    private Guid? CurrentUserOrNull()
    {
        try { return CurrentUser(); } catch { return null; }
    }

    private async Task<string?> Presign(string? key, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(tenant.TenantSlug)) return key;
        try { return await storage.GetPresignedUrlAsync(tenant.TenantSlug, key, ct); }
        catch { return key; }
    }

    private static int Pages(int total, int size) => total == 0 ? 0 : (int)Math.Ceiling(total / (double)size);
    private static string Csv(string? v) => string.IsNullOrEmpty(v) ? "" : $"\"{v.Replace("\"", "\"\"")}\"";
}
