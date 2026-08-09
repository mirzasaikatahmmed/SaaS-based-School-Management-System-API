using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.Payroll;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class SalaryPaymentService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http) : ISalaryPaymentService
{
    private static readonly Regex MonthRegex = new(@"^\d{4}-(0[1-9]|1[0-2])$", RegexOptions.Compiled);

    public async Task<SalaryPaymentListResponseDto> GetListAsync(SalaryPaymentFilterDto filter, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        ValidateMonth(filter.PaymentMonth);
        if (string.IsNullOrWhiteSpace(filter.Role))
            throw new AppException("Role is required.", 400);

        var page = filter.Page < 1 ? 1 : filter.Page;
        var size = filter.PageSize is < 1 or > 200 ? 25 : filter.PageSize;
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

        var data = new List<SalaryPaymentItemDto>();
        foreach (var (e, i) in employees.Select((e, i) => (e, i)))
        {
            assignments.TryGetValue(e.Id, out var assignment);
            var payment = await uow.SalaryPayments.GetByEmployeeAndMonthAsync(e.Id, filter.PaymentMonth, ct);

            string status;
            if (assignment is null)
                status = SalaryPaymentStatuses.NoGradeAssigned;
            else if (payment is not null && payment.Status == SalaryPaymentStatuses.Paid)
                status = SalaryPaymentStatuses.Paid;
            else
                status = SalaryPaymentStatuses.Unpaid;

            if (!string.IsNullOrWhiteSpace(filter.Status) &&
                !status.Equals(filter.Status, StringComparison.OrdinalIgnoreCase))
                continue;

            data.Add(new SalaryPaymentItemDto
            {
                EmployeeId = e.Id,
                PaymentId = payment?.Id,
                StaffId = e.StaffId,
                Name = e.Name,
                Designation = e.Designation?.Name,
                Department = e.Department?.Name,
                MobileNo = e.MobileNo,
                SalaryGrade = assignment?.Template?.SalaryGrade,
                BasicSalary = assignment?.Template?.BasicSalary,
                Status = status,
                PaymentDate = payment?.PaymentDate
            });
        }

        return new SalaryPaymentListResponseDto
        {
            Data = data,
            TotalCount = string.IsNullOrWhiteSpace(filter.Status) ? total : data.Count,
            Page = page,
            PageSize = size,
            TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)size)
        };
    }

    public async Task<SalaryPaymentResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var payment = await uow.SalaryPayments.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Salary payment '{id}' not found.");
        return MapPayment(payment);
    }

    public async Task<SalaryPaymentResponseDto> ProcessPaymentAsync(Guid employeeId, ProcessPaymentDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        ValidateMonth(dto.PaymentMonth);

        var employee = await uow.Employees.GetByIdWithDetailsAsync(employeeId, ct)
            ?? throw new NotFoundException($"Employee '{employeeId}' not found.");
        var assignment = await uow.SalaryAssignments.GetByEmployeeIdAsync(employeeId, ct)
            ?? throw new AppException("Employee must have a salary grade assigned before payment can be processed.", 400);
        var template = await uow.SalaryTemplates.GetByIdAsync(assignment.TemplateId, ct)
            ?? throw new NotFoundException("Assigned salary template not found.");

        var existing = await uow.SalaryPayments.GetByEmployeeAndMonthAsync(employeeId, dto.PaymentMonth, ct);
        if (existing is not null && existing.Status == SalaryPaymentStatuses.Paid)
            throw new AppException($"Payment already exists for employee and month {dto.PaymentMonth}.", 400);

        var overtimeRate = template.OvertimeRatePerHour ?? 0;
        var overtimeAmount = dto.OvertimeHours * overtimeRate;
        var approvedAdvance = await uow.AdvanceSalaries.SumApprovedForMonthAsync(employeeId, dto.PaymentMonth, ct);
        var advanceDeduction = dto.AdvanceDeduction > 0 ? dto.AdvanceDeduction : approvedAdvance;
        var finalAmount = template.NetSalary + overtimeAmount - advanceDeduction;

        SalaryPayment payment;
        if (existing is null)
        {
            payment = new SalaryPayment
            {
                Id = Guid.NewGuid(),
                EmployeeId = employeeId,
                TemplateId = template.Id,
                PaymentMonth = dto.PaymentMonth,
                BasicSalary = template.BasicSalary,
                TotalAllowance = template.TotalAllowance,
                TotalDeduction = template.TotalDeduction,
                NetSalary = template.NetSalary,
                OvertimeHours = dto.OvertimeHours,
                OvertimeAmount = overtimeAmount,
                AdvanceDeduction = advanceDeduction,
                FinalAmount = finalAmount,
                Status = SalaryPaymentStatuses.Paid,
                PaymentDate = DateTime.UtcNow,
                PaymentMethod = dto.PaymentMethod,
                PaymentNote = dto.PaymentNote,
                PaidBy = CurrentUserOrNull(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await uow.SalaryPayments.AddAsync(payment, ct);
        }
        else
        {
            payment = existing;
            payment.TemplateId = template.Id;
            payment.BasicSalary = template.BasicSalary;
            payment.TotalAllowance = template.TotalAllowance;
            payment.TotalDeduction = template.TotalDeduction;
            payment.NetSalary = template.NetSalary;
            payment.OvertimeHours = dto.OvertimeHours;
            payment.OvertimeAmount = overtimeAmount;
            payment.AdvanceDeduction = advanceDeduction;
            payment.FinalAmount = finalAmount;
            payment.Status = SalaryPaymentStatuses.Paid;
            payment.PaymentDate = DateTime.UtcNow;
            payment.PaymentMethod = dto.PaymentMethod;
            payment.PaymentNote = dto.PaymentNote;
            payment.PaidBy = CurrentUserOrNull();
            payment.UpdatedAt = DateTime.UtcNow;
            await uow.SalaryPayments.UpdateAsync(payment, ct);
        }

        await uow.SaveTenantChangesAsync(ct);
        payment.Employee = employee;
        payment.Template = template;
        return MapPayment(payment);
    }

    public async Task<SalaryPaymentResponseDto> UpdatePaymentAsync(Guid id, ProcessPaymentDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        ValidateMonth(dto.PaymentMonth);
        var payment = await uow.SalaryPayments.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Salary payment '{id}' not found.");

        if (!string.Equals(payment.PaymentMonth, dto.PaymentMonth, StringComparison.Ordinal))
        {
            var clash = await uow.SalaryPayments.GetByEmployeeAndMonthAsync(payment.EmployeeId, dto.PaymentMonth, ct);
            if (clash is not null && clash.Id != id)
                throw new AppException($"Payment already exists for employee and month {dto.PaymentMonth}.", 400);
            payment.PaymentMonth = dto.PaymentMonth;
        }

        var template = await uow.SalaryTemplates.GetByIdAsync(payment.TemplateId, ct)
            ?? throw new NotFoundException("Salary template not found.");
        var overtimeRate = template.OvertimeRatePerHour ?? 0;
        var overtimeAmount = dto.OvertimeHours * overtimeRate;

        payment.OvertimeHours = dto.OvertimeHours;
        payment.OvertimeAmount = overtimeAmount;
        payment.AdvanceDeduction = dto.AdvanceDeduction;
        payment.FinalAmount = payment.NetSalary + overtimeAmount - dto.AdvanceDeduction;
        payment.PaymentMethod = dto.PaymentMethod;
        payment.PaymentNote = dto.PaymentNote;
        payment.UpdatedAt = DateTime.UtcNow;
        await uow.SalaryPayments.UpdateAsync(payment, ct);
        await uow.SaveTenantChangesAsync(ct);
        return MapPayment(await uow.SalaryPayments.GetByIdAsync(id, ct) ?? payment);
    }

    public async Task<(byte[] Content, string ContentType, string FileName)> ExportAsync(SalaryPaymentFilterDto filter, CancellationToken ct = default)
    {
        var list = await GetListAsync(new SalaryPaymentFilterDto
        {
            Role = filter.Role,
            PaymentMonth = filter.PaymentMonth,
            Search = filter.Search,
            Status = filter.Status,
            Page = 1,
            PageSize = 500
        }, ct);

        var sb = new StringBuilder("StaffId,Name,Designation,Department,MobileNo,SalaryGrade,BasicSalary,Status,PaymentDate\n");
        foreach (var x in list.Data)
        {
            sb.AppendLine($"{Csv(x.StaffId)},{Csv(x.Name)},{Csv(x.Designation)},{Csv(x.Department)},{Csv(x.MobileNo)},{Csv(x.SalaryGrade)},{x.BasicSalary},{Csv(x.Status)},{x.PaymentDate:O}");
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        var fmt = (filter.Export ?? "csv").ToLowerInvariant();
        return fmt switch
        {
            "csv" => (bytes, "text/csv", $"salary-payment-{filter.PaymentMonth}.csv"),
            "excel" => (bytes, "application/vnd.ms-excel", $"salary-payment-{filter.PaymentMonth}.xls"),
            "pdf" => (bytes, "application/pdf", $"salary-payment-{filter.PaymentMonth}.pdf"),
            _ => throw new AppException("Unsupported export format. Use csv, excel, or pdf.", 400)
        };
    }

    public async Task<MySalaryDto> GetMySalaryAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        var employee = await CurrentEmployee(ct);
        var assignment = await uow.SalaryAssignments.GetByEmployeeIdAsync(employee.Id, ct);
        if (assignment is null)
            return new MySalaryDto();

        var template = await uow.SalaryTemplates.GetByIdAsync(assignment.TemplateId, ct)
            ?? throw new NotFoundException("Assigned salary template not found.");

        return new MySalaryDto
        {
            AssignmentId = assignment.Id,
            TemplateId = template.Id,
            SalaryGrade = template.SalaryGrade,
            BasicSalary = template.BasicSalary,
            TotalAllowance = template.TotalAllowance,
            TotalDeduction = template.TotalDeduction,
            NetSalary = template.NetSalary,
            OvertimeRatePerHour = template.OvertimeRatePerHour,
            Allowances = template.Allowances.OrderBy(a => a.SortOrder).Select(a => new AllowanceRowDto
            {
                Id = a.Id, Name = a.Name, Amount = a.Amount, SortOrder = a.SortOrder
            }).ToList(),
            Deductions = template.Deductions.OrderBy(d => d.SortOrder).Select(d => new DeductionRowDto
            {
                Id = d.Id, Name = d.Name, Amount = d.Amount, SortOrder = d.SortOrder
            }).ToList()
        };
    }

    public async Task<SalaryPaymentResponseDto> GetMySalaryForMonthAsync(string month, CancellationToken ct = default)
    {
        await Ready(ct);
        ValidateMonth(month);
        var employee = await CurrentEmployee(ct);
        var payment = await uow.SalaryPayments.GetByEmployeeAndMonthAsync(employee.Id, month, ct)
            ?? throw new NotFoundException($"No salary payment found for {month}.");
        return MapPayment(payment);
    }

    private static SalaryPaymentResponseDto MapPayment(SalaryPayment p) => new()
    {
        Id = p.Id,
        EmployeeId = p.EmployeeId,
        StaffId = p.Employee?.StaffId ?? string.Empty,
        EmployeeName = p.Employee?.Name ?? string.Empty,
        TemplateId = p.TemplateId,
        SalaryGrade = p.Template?.SalaryGrade ?? string.Empty,
        PaymentMonth = p.PaymentMonth,
        BasicSalary = p.BasicSalary,
        TotalAllowance = p.TotalAllowance,
        TotalDeduction = p.TotalDeduction,
        NetSalary = p.NetSalary,
        OvertimeHours = p.OvertimeHours,
        OvertimeAmount = p.OvertimeAmount,
        AdvanceDeduction = p.AdvanceDeduction,
        FinalAmount = p.FinalAmount,
        Status = p.Status,
        PaymentDate = p.PaymentDate,
        PaymentMethod = p.PaymentMethod,
        PaymentNote = p.PaymentNote,
        CreatedAt = p.CreatedAt
    };

    private static void ValidateMonth(string? month)
    {
        if (string.IsNullOrWhiteSpace(month) || !MonthRegex.IsMatch(month))
            throw new AppException("PaymentMonth must be in YYYY-MM format.", 400);
    }

    private async Task<Employee> CurrentEmployee(CancellationToken ct)
    {
        var userId = CurrentUser();
        return await uow.Employees.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException("Employee profile not found for current user.");
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
        return p.FindAll("role").Concat(p.FindAll(ClaimTypes.Role))
            .Select(x => x.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private void Manage()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) &&
            !r.Contains(AppConstants.Roles.SuperAdmin) &&
            !r.Contains(AppConstants.Roles.Accountant))
            throw new ForbiddenException("Only Super Admin, School Admin, or Accountant can manage salary payments.");
    }

    private Guid CurrentUser()
    {
        var c = http.HttpContext?.User.FindFirst(AppConstants.Claims.UserId)
            ?? http.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        if (c is null || !Guid.TryParse(c.Value, out var id))
            throw new UnauthorizedException();
        return id;
    }

    private Guid? CurrentUserOrNull()
    {
        try { return CurrentUser(); }
        catch { return null; }
    }

    private static string Csv(string? v) => string.IsNullOrEmpty(v) ? "" : $"\"{v.Replace("\"", "\"\"")}\"";
}
