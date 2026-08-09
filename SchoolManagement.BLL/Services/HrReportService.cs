using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.Reports;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class HrReportService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http) : IHrReportService
{
    private static readonly Regex MonthRegex = new(@"^\d{4}-(0[1-9]|1[0-2])$", RegexOptions.Compiled);

    public async Task<LeaveReportDto> GetLeaveReportAsync(LeaveReportFilterDto filter, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();

        if (filter.ToDate.Date < filter.FromDate.Date)
            throw new AppException("toDate must be on or after fromDate.", 400);

        var page = filter.Page < 1 ? 1 : filter.Page;
        var size = string.Equals(filter.Export, "csv", StringComparison.OrdinalIgnoreCase)
            ? Math.Clamp(filter.PageSize is < 1 ? 5000 : filter.PageSize, 1, 5000)
            : Math.Clamp(filter.PageSize is < 1 ? 100 : filter.PageSize, 1, 500);

        var (items, total) = await uow.LeaveRequests.SearchAsync(new LeaveRequestSearchFilter
        {
            Role = filter.Role,
            Status = filter.Status,
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            Search = filter.Search,
            Page = page,
            PageSize = size
        }, ct);

        var rows = items.Select((x, i) => new LeaveReportRowDto
        {
            Sl = (page - 1) * size + i + 1,
            Id = x.Id,
            Role = x.Employee.Role,
            Applicant = x.Employee.Name,
            EmployeeId = x.EmployeeId,
            LeaveCategory = x.LeaveCategory.Name,
            DateOfStart = x.DateOfStart,
            DateOfEnd = x.DateOfEnd,
            Days = x.Days,
            ApplyDate = x.ApplyDate,
            Status = x.Status
        }).ToList();

        return new LeaveReportDto
        {
            Role = filter.Role,
            FromDate = filter.FromDate.Date,
            ToDate = filter.ToDate.Date,
            Rows = rows,
            TotalCount = total,
            Page = page,
            PageSize = size,
            TotalPages = (int)Math.Ceiling(total / (double)size)
        };
    }

    public async Task<PayrollSummaryReportDto> GetPayrollSummaryAsync(PayrollSummaryFilterDto filter, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();

        if (string.IsNullOrWhiteSpace(filter.Month) || !MonthRegex.IsMatch(filter.Month.Trim()))
            throw new AppException("Month must be YYYY-MM.", 400);

        var month = filter.Month.Trim();
        var page = filter.Page < 1 ? 1 : filter.Page;
        var size = string.Equals(filter.Export, "csv", StringComparison.OrdinalIgnoreCase)
            ? Math.Clamp(filter.PageSize is < 1 ? 5000 : filter.PageSize, 1, 5000)
            : Math.Clamp(filter.PageSize is < 1 ? 100 : filter.PageSize, 1, 500);

        var (employees, total) = await uow.Employees.SearchAsync(new EmployeeSearchFilter
        {
            Role = filter.Role,
            Search = filter.Search,
            IsActive = true,
            Page = page,
            PageSize = size
        }, ct);

        var assignments = (await uow.SalaryAssignments.GetByEmployeeIdsAsync(employees.Select(e => e.Id), ct))
            .ToDictionary(a => a.EmployeeId);
        var payments = (await uow.SalaryPayments.GetByMonthAsync(month, filter.Role, ct))
            .ToDictionary(p => p.EmployeeId);

        var rows = new List<PayrollSummaryRowDto>();
        decimal tSal = 0, tAll = 0, tDed = 0, tNet = 0;
        var sl = (page - 1) * size;

        foreach (var e in employees)
        {
            assignments.TryGetValue(e.Id, out var assignment);
            payments.TryGetValue(e.Id, out var payment);

            decimal salary, allowance, deduction, net;
            string? payVia;
            string status;

            if (payment is not null)
            {
                salary = payment.BasicSalary;
                allowance = payment.TotalAllowance;
                deduction = payment.TotalDeduction;
                net = payment.FinalAmount != 0 ? payment.FinalAmount : payment.NetSalary;
                payVia = payment.PaymentMethod;
                status = payment.Status;
            }
            else if (assignment?.Template is not null)
            {
                salary = assignment.Template.BasicSalary;
                allowance = assignment.Template.TotalAllowance;
                deduction = assignment.Template.TotalDeduction;
                net = assignment.Template.NetSalary;
                payVia = null;
                status = SalaryPaymentStatuses.Unpaid;
            }
            else
            {
                salary = allowance = deduction = net = 0;
                payVia = null;
                status = SalaryPaymentStatuses.NoGradeAssigned;
            }

            tSal += salary;
            tAll += allowance;
            tDed += deduction;
            tNet += net;

            rows.Add(new PayrollSummaryRowDto
            {
                Sl = ++sl,
                EmployeeId = e.Id,
                PaymentId = payment?.Id,
                Name = e.Name,
                Designation = e.Designation?.Name,
                Salary = salary,
                Allowance = allowance,
                Deduction = deduction,
                NetSalary = net,
                PayVia = payVia,
                Status = status
            });
        }

        return new PayrollSummaryReportDto
        {
            Month = month,
            Role = filter.Role,
            Rows = rows,
            TotalSalary = tSal,
            TotalAllowance = tAll,
            TotalDeduction = tDed,
            TotalNetSalary = tNet,
            TotalCount = total,
            Page = page,
            PageSize = size,
            TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)size)
        };
    }

    private async Task Ready(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(tenant.SchemaName))
            throw new AppException("X-Tenant-ID header is required.", 400);
        await provisioner.EnsureSettingsModuleAsync(tenant.SchemaName!, ct);
    }

    private HashSet<string> Roles() =>
        http.HttpContext?.User.FindAll("role").Concat(http.HttpContext.User.FindAll(ClaimTypes.Role))
            .Select(x => x.Value).ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];

    private void Manage()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin)
            && !r.Contains(AppConstants.Roles.Accountant))
            throw new ForbiddenException("Only Super Admin, School Admin, or Accountant can access HR reports.");
    }
}
