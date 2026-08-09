using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.AdvanceSalary;
using SchoolManagement.BLL.DTOs.Leave;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class LeaveService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IStorageService storage,
    IHttpContextAccessor http) : ILeaveService
{
    public async Task<LeaveListResponseDto> GetMyListAsync(LeaveFilterDto filter, CancellationToken ct = default)
    {
        await Ready(ct);
        var employee = await CurrentEmployee(ct);
        return await Search(new LeaveRequestSearchFilter
        {
            EmployeeId = employee.Id,
            Status = filter.Status,
            Search = filter.Search,
            Page = filter.Page,
            PageSize = filter.PageSize
        }, includeRole: false, ct);
    }

    public async Task<LeaveListItemDto> CreateMyAsync(CreateLeaveRequestDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        var employee = await CurrentEmployee(ct);
        return await CreateInternal(employee, dto.LeaveCategoryId, dto.DateOfStart, dto.DateOfEnd, dto.Reason, null, ct);
    }

    public async Task CancelMyAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        var employee = await CurrentEmployee(ct);
        var leave = await uow.LeaveRequests.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Leave request '{id}' not found.");
        if (leave.EmployeeId != employee.Id)
            throw new ForbiddenException("You can only cancel your own leave requests.");
        if (leave.Status != HrRequestStatuses.Pending)
            throw new AppException("Only pending leaves can be cancelled", 400);
        await uow.LeaveRequests.DeleteAsync(leave, ct);
        await uow.SaveTenantChangesAsync(ct);
    }

    public async Task<LeaveListResponseDto> GetManageListAsync(LeaveManageFilterDto filter, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        return await Search(new LeaveRequestSearchFilter
        {
            Role = filter.Role,
            Status = filter.Status,
            Search = filter.Search,
            Page = filter.Page,
            PageSize = filter.PageSize
        }, includeRole: true, ct);
    }

    public async Task<LeaveListItemDto> AdminCreateAsync(AdminCreateLeaveRequestDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var employee = await uow.Employees.GetByIdWithDetailsAsync(dto.EmployeeId, ct)
            ?? throw new NotFoundException($"Employee '{dto.EmployeeId}' not found.");
        return await CreateInternal(employee, dto.LeaveCategoryId, dto.DateOfStart, dto.DateOfEnd, dto.Reason, dto.Comments, ct);
    }

    public async Task<LeaveListItemDto> ApproveAsync(Guid id, ReviewLeaveDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var leave = await RequirePending(id, ct);
        leave.Status = HrRequestStatuses.Approved;
        leave.Comments = dto.Comments;
        leave.ReviewedBy = CurrentUserOrNull();
        leave.ReviewedAt = DateTime.UtcNow;
        leave.UpdatedAt = DateTime.UtcNow;
        await uow.LeaveRequests.UpdateAsync(leave, ct);
        await uow.SaveTenantChangesAsync(ct);
        return await Map(await uow.LeaveRequests.GetByIdAsync(id, ct) ?? leave, 0, true, ct);
    }

    public async Task<LeaveListItemDto> RejectAsync(Guid id, ReviewLeaveDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        if (string.IsNullOrWhiteSpace(dto.Comments))
            throw new AppException("Comments is required when rejecting leave.", 400);
        var leave = await RequirePending(id, ct);
        leave.Status = HrRequestStatuses.Rejected;
        leave.Comments = dto.Comments.Trim();
        leave.ReviewedBy = CurrentUserOrNull();
        leave.ReviewedAt = DateTime.UtcNow;
        leave.UpdatedAt = DateTime.UtcNow;
        await uow.LeaveRequests.UpdateAsync(leave, ct);
        await uow.SaveTenantChangesAsync(ct);
        return await Map(await uow.LeaveRequests.GetByIdAsync(id, ct) ?? leave, 0, true, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var leave = await uow.LeaveRequests.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Leave request '{id}' not found.");
        await uow.LeaveRequests.DeleteAsync(leave, ct);
        await uow.SaveTenantChangesAsync(ct);
    }

    public async Task<LeaveListItemDto> UploadAttachmentAsync(Guid id, Stream stream, string fileName, string contentType, CancellationToken ct = default)
    {
        await Ready(ct);
        var leave = await uow.LeaveRequests.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Leave request '{id}' not found.");
        await EnsureManageOrOwnerAsync(leave, ct);

        if (stream.CanSeek && stream.Length > 5 * 1024 * 1024)
            throw new AppException("Attachment must be 5MB or smaller.", 400);

        var slug = tenant.TenantSlug ?? throw new AppException("Tenant slug is not resolved.", 400);
        var safeName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safeName)) safeName = "attachment.bin";
        var key = $"{AppConstants.StorageFolders.LeaveAttachments}/{id}/{safeName}";

        if (!string.IsNullOrWhiteSpace(leave.AttachmentUrl))
        {
            try { await storage.DeleteFileAsync(slug, leave.AttachmentUrl, ct); } catch { /* ignore */ }
        }

        await storage.UploadObjectAsync(slug, key, stream, contentType, ct);
        leave.AttachmentUrl = key;
        leave.UpdatedAt = DateTime.UtcNow;
        await uow.LeaveRequests.UpdateAsync(leave, ct);
        await uow.SaveTenantChangesAsync(ct);
        return await Map(await uow.LeaveRequests.GetByIdAsync(id, ct) ?? leave, 0, true, ct);
    }

    public async Task<(byte[] Content, string ContentType, string FileName)> ExportAsync(LeaveManageFilterDto filter, CancellationToken ct = default)
    {
        var list = await GetManageListAsync(new LeaveManageFilterDto
        {
            Role = filter.Role,
            Status = filter.Status,
            Search = filter.Search,
            Page = 1,
            PageSize = 500
        }, ct);

        var sb = new StringBuilder("Role,ApplicantName,LeaveCategory,DateOfStart,DateOfEnd,Days,Status,ApplyDate\n");
        foreach (var x in list.Data)
            sb.AppendLine($"{Csv(x.Role)},{Csv(x.ApplicantName)},{Csv(x.LeaveCategory)},{x.DateOfStart:yyyy-MM-dd},{x.DateOfEnd:yyyy-MM-dd},{x.Days},{Csv(x.Status)},{x.ApplyDate:O}");

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        var fmt = (filter.Export ?? "csv").ToLowerInvariant();
        return fmt switch
        {
            "csv" => (bytes, "text/csv", $"leave-{DateTime.UtcNow:yyyyMMdd}.csv"),
            "excel" => (bytes, "application/vnd.ms-excel", $"leave-{DateTime.UtcNow:yyyyMMdd}.xls"),
            "pdf" => (bytes, "application/pdf", $"leave-{DateTime.UtcNow:yyyyMMdd}.pdf"),
            _ => throw new AppException("Unsupported export format. Use csv, excel, or pdf.", 400)
        };
    }

    public async Task<IReadOnlyList<LeaveCategoryLookupDto>> GetMyLeaveTypesAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        var employee = await CurrentEmployee(ct);
        var items = await uow.LeaveCategories.GetByRoleAsync(employee.Role, ct);
        return items.Select(c => new LeaveCategoryLookupDto
        {
            Id = c.Id,
            Name = c.Name,
            Days = c.Days,
            Role = c.Role
        }).ToList();
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

    private async Task<LeaveListItemDto> CreateInternal(
        Employee employee,
        Guid categoryId,
        DateTime start,
        DateTime end,
        string? reason,
        string? comments,
        CancellationToken ct)
    {
        if (end.Date < start.Date)
            throw new AppException("DateOfEnd must be greater than or equal to DateOfStart.", 400);

        var category = await uow.LeaveCategories.GetByIdAsync(categoryId, ct)
            ?? throw new NotFoundException($"Leave category '{categoryId}' not found.");
        if (!category.IsActive)
            throw new AppException("Leave category is inactive.", 400);
        if (!category.Role.Equals(employee.Role, StringComparison.OrdinalIgnoreCase))
            throw new AppException($"Leave category '{category.Name}' is not available for role '{employee.Role}'.", 400);

        var days = (end.Date - start.Date).Days + 1;
        var year = start.Year;
        var used = await uow.LeaveRequests.SumUsedDaysAsync(employee.Id, categoryId, year, ct);
        var available = category.Days - used;
        if (days > available)
            throw new AppException(
                $"Requested days exceed available quota. Available: {Math.Max(0, available)} days, Requested: {days} days",
                400);

        var entity = new LeaveRequest
        {
            Id = Guid.NewGuid(),
            EmployeeId = employee.Id,
            LeaveCategoryId = categoryId,
            DateOfStart = DateTime.SpecifyKind(start.Date, DateTimeKind.Utc),
            DateOfEnd = DateTime.SpecifyKind(end.Date, DateTimeKind.Utc),
            Days = days,
            Reason = reason,
            Comments = comments,
            Status = HrRequestStatuses.Pending,
            ApplyDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await uow.LeaveRequests.AddAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
        return await Map(await uow.LeaveRequests.GetByIdAsync(entity.Id, ct) ?? entity, 0, true, ct);
    }

    private async Task<LeaveRequest> RequirePending(Guid id, CancellationToken ct)
    {
        var leave = await uow.LeaveRequests.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Leave request '{id}' not found.");
        if (leave.Status != HrRequestStatuses.Pending)
            throw new AppException($"Cannot review a leave that is already {leave.Status}.", 400);
        return leave;
    }

    private async Task<LeaveListResponseDto> Search(LeaveRequestSearchFilter filter, bool includeRole, CancellationToken ct)
    {
        var page = filter.Page < 1 ? 1 : filter.Page;
        var size = filter.PageSize is < 1 or > 200 ? 25 : filter.PageSize;
        filter.Page = page;
        filter.PageSize = size;
        var (items, total) = await uow.LeaveRequests.SearchAsync(filter, ct);
        var data = new List<LeaveListItemDto>();
        for (var i = 0; i < items.Count; i++)
            data.Add(await Map(items[i], (page - 1) * size + i + 1, includeRole, ct));

        return new LeaveListResponseDto
        {
            Data = data,
            TotalCount = total,
            Page = page,
            PageSize = size,
            TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)size)
        };
    }

    private async Task<LeaveListItemDto> Map(LeaveRequest x, int sl, bool includeRole, CancellationToken ct) => new()
    {
        Id = x.Id,
        Sl = sl,
        Role = includeRole ? x.Employee?.Role : null,
        ApplicantName = x.Employee?.Name ?? string.Empty,
        LeaveCategory = x.LeaveCategory?.Name ?? string.Empty,
        DateOfStart = x.DateOfStart,
        DateOfEnd = x.DateOfEnd,
        Days = x.Days,
        ApplyDate = x.ApplyDate,
        Status = x.Status,
        Comments = x.Comments,
        AttachmentUrl = await Presign(x.AttachmentUrl, ct)
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
        return p.FindAll("role").Concat(p.FindAll(ClaimTypes.Role)).Select(x => x.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private void Manage()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin))
            throw new ForbiddenException("Only Super Admin or School Admin can manage leave applications.");
    }

    private async Task EnsureManageOrOwnerAsync(LeaveRequest leave, CancellationToken ct)
    {
        var r = Roles();
        if (r.Contains(AppConstants.Roles.Admin) || r.Contains(AppConstants.Roles.SuperAdmin))
            return;
        var employee = await CurrentEmployee(ct);
        if (leave.EmployeeId != employee.Id)
            throw new ForbiddenException("You do not have access to this leave request.");
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

    private static string Csv(string? v) => string.IsNullOrEmpty(v) ? "" : $"\"{v.Replace("\"", "\"\"")}\"";
}
