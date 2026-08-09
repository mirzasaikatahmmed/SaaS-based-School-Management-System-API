using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.Attendance;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class EmployeeAttendanceService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http) : IEmployeeAttendanceService
{
    public static readonly string[] Statuses = ["Present", "Absent", "Late", "HalfDay"];

    public async Task<EmployeeAttendanceForDateResponseDto> GetForDateAsync(string? role, DateTime date, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();

        var employees = await uow.EmployeeAttendances.GetActiveEmployeesByRoleAsync(role, ct);
        var existing = await uow.EmployeeAttendances.GetForDateAsync(role, date, ct);
        var byEmployee = existing.ToDictionary(a => a.EmployeeId);

        var items = employees.Select((e, i) =>
        {
            byEmployee.TryGetValue(e.Id, out var a);
            return new EmployeeAttendanceRowDto
            {
                Id = a?.Id,
                Sl = i + 1,
                EmployeeId = e.Id,
                EmployeeName = e.Name,
                StaffId = e.StaffId,
                Role = e.Role,
                Status = a?.Status ?? "Present",
                Remarks = a?.Remarks
            };
        }).ToList();

        return new EmployeeAttendanceForDateResponseDto { Role = role, AttendanceDate = date.Date, Items = items };
    }

    public async Task<EmployeeAttendanceForDateResponseDto> SaveAsync(SaveEmployeeAttendanceDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();

        if (dto.Items.Count == 0)
            throw new AppException("At least one attendance record is required.", 400);

        foreach (var item in dto.Items)
        {
            if (!Statuses.Contains(item.Status, StringComparer.OrdinalIgnoreCase))
                throw new AppException($"Invalid attendance status '{item.Status}'.", 400);
        }

        var userId = TryCurrentUser();
        var entries = dto.Items.Select(i => new EmployeeAttendance
        {
            Id = Guid.NewGuid(),
            EmployeeId = i.EmployeeId,
            AttendanceDate = dto.AttendanceDate.Date,
            Status = Statuses.First(s => s.Equals(i.Status, StringComparison.OrdinalIgnoreCase)),
            Remarks = i.Remarks?.Trim(),
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }).ToList();

        await uow.BeginTenantTransactionAsync(ct);
        try
        {
            await uow.EmployeeAttendances.UpsertBatchAsync(entries, ct);
            await uow.SaveTenantChangesAsync(ct);
            await uow.CommitTenantTransactionAsync(ct);
        }
        catch
        {
            await uow.RollbackTenantTransactionAsync(ct);
            throw;
        }

        return await GetForDateAsync(dto.Role, dto.AttendanceDate, ct);
    }

    public async Task<EmployeeAttendanceReportResponseDto> GetReportAsync(EmployeeAttendanceReportFilterDto filter, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        return await BuildReport(filter.Role, null, filter.FromDate, filter.ToDate, ct);
    }

    public async Task<EmployeeAttendanceReportResponseDto> GetMyReportAsync(EmployeeAttendanceReportFilterDto filter, CancellationToken ct = default)
    {
        await Ready(ct);
        var employee = await uow.Employees.GetByUserIdAsync(CurrentUser(), ct)
            ?? throw new NotFoundException("No employee profile found for current user.");
        return await BuildReport(null, employee.Id, filter.FromDate, filter.ToDate, ct);
    }

    private async Task<EmployeeAttendanceReportResponseDto> BuildReport(
        string? role, Guid? employeeId, DateTime fromDate, DateTime toDate, CancellationToken ct)
    {
        var records = await uow.EmployeeAttendances.GetReportAsync(role, employeeId, fromDate, toDate, ct);

        var rows = records.Select(a => new EmployeeAttendanceReportRowDto
        {
            AttendanceDate = a.AttendanceDate,
            EmployeeId = a.EmployeeId,
            EmployeeName = a.Employee.Name,
            StaffId = a.Employee.StaffId,
            Role = a.Employee.Role,
            Status = a.Status ?? string.Empty,
            Remarks = a.Remarks
        }).ToList();

        var summary = records.GroupBy(a => a.EmployeeId).Select(g =>
        {
            var first = g.First();
            return new EmployeeAttendanceSummaryDto
            {
                EmployeeId = g.Key,
                EmployeeName = first.Employee.Name,
                StaffId = first.Employee.StaffId,
                PresentCount = g.Count(x => x.Status == "Present"),
                AbsentCount = g.Count(x => x.Status == "Absent"),
                LateCount = g.Count(x => x.Status == "Late"),
                HalfDayCount = g.Count(x => x.Status == "HalfDay"),
                TotalDays = g.Count()
            };
        }).ToList();

        return new EmployeeAttendanceReportResponseDto { Rows = rows, Summary = summary };
    }

    private async Task Ready(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(tenant.SchemaName))
            throw new AppException("X-Tenant-ID header is required.", 400);
        await provisioner.EnsureGradesAttendanceLibraryEventsModuleAsync(tenant.SchemaName!, ct);
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
            throw new ForbiddenException("Only Super Admin or School Admin can manage employee attendance.");
    }

    private Guid CurrentUser()
    {
        var c = http.HttpContext?.User.FindFirst(AppConstants.Claims.UserId)
            ?? http.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        if (c is null || !Guid.TryParse(c.Value, out var id)) throw new UnauthorizedException();
        return id;
    }

    private Guid? TryCurrentUser()
    {
        var c = http.HttpContext?.User.FindFirst(AppConstants.Claims.UserId)
            ?? http.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        return c is not null && Guid.TryParse(c.Value, out var id) ? id : null;
    }
}
