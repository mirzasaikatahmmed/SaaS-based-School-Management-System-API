using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.Award;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class AwardService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http) : IAwardService
{
    public async Task<AwardListResponseDto> GetListAsync(AwardFilterDto filter, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        return await Search(new AwardSearchFilter
        {
            Role = filter.Role,
            Search = filter.Search,
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            Page = filter.Page,
            PageSize = filter.PageSize
        }, ct);
    }

    public async Task<AwardListResponseDto> GetMyAwardsAsync(AwardFilterDto filter, CancellationToken ct = default)
    {
        await Ready(ct);
        var userId = CurrentUser();
        var employee = await uow.Employees.GetByUserIdAsync(userId, ct);
        if (employee is not null)
        {
            return await Search(new AwardSearchFilter
            {
                EmployeeId = employee.Id,
                Search = filter.Search,
                FromDate = filter.FromDate,
                ToDate = filter.ToDate,
                Page = filter.Page,
                PageSize = filter.PageSize
            }, ct);
        }

        var student = await uow.Students.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException("No employee or student profile found for current user.");
        return await Search(new AwardSearchFilter
        {
            StudentId = student.Id,
            Search = filter.Search,
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            Page = filter.Page,
            PageSize = filter.PageSize
        }, ct);
    }

    public async Task<AwardResponseDto> GiveAwardAsync(GiveAwardDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var role = CanonicalRole(dto.Role);
        var givenDate = dto.GivenDate is null || dto.GivenDate == default
            ? DateTime.UtcNow.Date
            : DateTime.SpecifyKind(dto.GivenDate.Value.Date, DateTimeKind.Utc);

        var award = new Award
        {
            Id = Guid.NewGuid(),
            Role = role,
            AwardName = dto.AwardName.Trim(),
            GiftItem = dto.GiftItem.Trim(),
            CashPrice = dto.CashPrice,
            AwardReason = dto.AwardReason.Trim(),
            GivenDate = givenDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await ApplyRecipient(award, role, dto.EmployeeId, dto.StudentId, ct);
        await uow.Awards.AddAsync(award, ct);
        await uow.SaveTenantChangesAsync(ct);
        return MapDetail(await uow.Awards.GetByIdAsync(award.Id, ct) ?? award, 0);
    }

    public async Task<AwardResponseDto> UpdateAsync(Guid id, UpdateAwardDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var award = await uow.Awards.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Award '{id}' not found.");
        var role = CanonicalRole(dto.Role);

        award.Role = role;
        award.AwardName = dto.AwardName.Trim();
        award.GiftItem = dto.GiftItem.Trim();
        award.CashPrice = dto.CashPrice;
        award.AwardReason = dto.AwardReason.Trim();
        award.GivenDate = DateTime.SpecifyKind(dto.GivenDate.Date, DateTimeKind.Utc);
        award.UpdatedAt = DateTime.UtcNow;

        await ApplyRecipient(award, role, dto.EmployeeId, dto.StudentId, ct);
        await uow.Awards.UpdateAsync(award, ct);
        await uow.SaveTenantChangesAsync(ct);
        return MapDetail(await uow.Awards.GetByIdAsync(id, ct) ?? award, 0);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var award = await uow.Awards.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Award '{id}' not found.");
        await uow.Awards.DeleteAsync(award, ct);
        await uow.SaveTenantChangesAsync(ct);
    }

    public async Task<(byte[] Content, string ContentType, string FileName)> ExportAsync(AwardFilterDto filter, CancellationToken ct = default)
    {
        var list = await GetListAsync(new AwardFilterDto
        {
            Role = filter.Role,
            Search = filter.Search,
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            Page = 1,
            PageSize = 500
        }, ct);

        var sb = new StringBuilder("Branch,Winner,Role,AwardName,GiftItem,CashPrice,AwardReason,GivenDate\n");
        foreach (var x in list.Data)
            sb.AppendLine($"{Csv(x.Branch)},{Csv(x.Winner)},{Csv(x.Role)},{Csv(x.AwardName)},{Csv(x.GiftItem)},{x.CashPrice},{Csv(x.AwardReason)},{x.GivenDate:yyyy-MM-dd}");

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        var fmt = (filter.Export ?? "csv").ToLowerInvariant();
        return fmt switch
        {
            "csv" => (bytes, "text/csv", $"awards-{DateTime.UtcNow:yyyyMMdd}.csv"),
            "excel" => (bytes, "application/vnd.ms-excel", $"awards-{DateTime.UtcNow:yyyyMMdd}.xls"),
            "pdf" => (bytes, "application/pdf", $"awards-{DateTime.UtcNow:yyyyMMdd}.pdf"),
            _ => throw new AppException("Unsupported export format. Use csv, excel, or pdf.", 400)
        };
    }

    public async Task<IReadOnlyList<WinnerLookupDto>> GetWinnersLookupAsync(string role, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        if (string.IsNullOrWhiteSpace(role))
            throw new AppException("Role is required.", 400);

        var canonical = CanonicalRole(role);
        if (AwardRoles.IsStudent(canonical))
        {
            var (students, _) = await uow.Students.SearchAsync(new StudentSearchFilter
            {
                IsActive = true,
                Page = 1,
                PageSize = 500
            }, ct);
            return students.Select(s => new WinnerLookupDto
            {
                Id = s.Id,
                DisplayName = $"{StudentName(s)} ({s.RegisterNo})",
                RecipientType = "Student"
            }).ToList();
        }

        var (employees, _) = await uow.Employees.SearchAsync(new EmployeeSearchFilter
        {
            Role = canonical,
            IsActive = true,
            Page = 1,
            PageSize = 500
        }, ct);
        return employees.Select(e => new WinnerLookupDto
        {
            Id = e.Id,
            DisplayName = $"{e.Name} ({e.StaffId})",
            RecipientType = "Employee"
        }).ToList();
    }

    private async Task ApplyRecipient(Award award, string role, Guid? employeeId, Guid? studentId, CancellationToken ct)
    {
        if (AwardRoles.IsStudent(role))
        {
            if (!studentId.HasValue || employeeId.HasValue)
                throw new AppException("Provide EmployeeId for staff roles or StudentId for Student role", 400);
            var student = await uow.Students.GetByIdAsync(studentId.Value, ct)
                ?? throw new NotFoundException("Student not found");
            award.StudentId = student.Id;
            award.EmployeeId = null;
            return;
        }

        if (!employeeId.HasValue || studentId.HasValue)
            throw new AppException("Provide EmployeeId for staff roles or StudentId for Student role", 400);
        var employee = await uow.Employees.GetByIdAsync(employeeId.Value, ct)
            ?? throw new NotFoundException("Employee not found");
        if (!employee.Role.Equals(role, StringComparison.OrdinalIgnoreCase))
            throw new AppException("Employee role does not match selected role", 400);
        award.EmployeeId = employee.Id;
        award.StudentId = null;
    }

    private async Task<AwardListResponseDto> Search(AwardSearchFilter filter, CancellationToken ct)
    {
        var page = filter.Page < 1 ? 1 : filter.Page;
        var size = filter.PageSize is < 1 or > 200 ? 25 : filter.PageSize;
        filter.Page = page;
        filter.PageSize = size;
        var (items, total) = await uow.Awards.SearchAsync(filter, ct);
        return new AwardListResponseDto
        {
            Data = items.Select((a, i) => MapList(a, (page - 1) * size + i + 1)).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = size,
            TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)size)
        };
    }

    private AwardListItemDto MapList(Award a, int sl) => new()
    {
        Id = a.Id,
        Sl = sl,
        Branch = tenant.TenantName ?? string.Empty,
        Winner = ResolveWinner(a),
        Role = a.Role,
        AwardName = a.AwardName,
        GiftItem = a.GiftItem,
        CashPrice = a.CashPrice,
        AwardReason = a.AwardReason,
        GivenDate = a.GivenDate
    };

    private AwardResponseDto MapDetail(Award a, int sl)
    {
        var list = MapList(a, sl);
        return new AwardResponseDto
        {
            Id = list.Id,
            Sl = list.Sl,
            Branch = list.Branch,
            Winner = list.Winner,
            Role = list.Role,
            AwardName = list.AwardName,
            GiftItem = list.GiftItem,
            CashPrice = list.CashPrice,
            AwardReason = list.AwardReason,
            GivenDate = list.GivenDate,
            EmployeeId = a.EmployeeId,
            StudentId = a.StudentId,
            RecipientType = a.StudentId.HasValue ? "Student" : "Employee",
            CreatedAt = a.CreatedAt
        };
    }

    private static string ResolveWinner(Award a)
    {
        if (a.Employee is not null) return a.Employee.Name;
        if (a.Student is not null) return StudentName(a.Student);
        return string.Empty;
    }

    private static string StudentName(Student s)
        => string.IsNullOrWhiteSpace(s.LastName) ? s.FirstName.Trim() : $"{s.FirstName.Trim()} {s.LastName.Trim()}";

    private static string CanonicalRole(string role)
        => AwardRoles.All.FirstOrDefault(x => x.Equals(role.Trim(), StringComparison.OrdinalIgnoreCase))
           ?? throw new AppException("Invalid award role.", 400);

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
            throw new ForbiddenException("Only Super Admin, School Admin, or Accountant can manage awards.");
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
