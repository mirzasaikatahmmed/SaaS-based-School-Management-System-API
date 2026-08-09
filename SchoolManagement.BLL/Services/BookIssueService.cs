using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.Library;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class BookIssueService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http) : IBookIssueService
{
    private const string StatusIssued = "Issued";
    private const string StatusReturned = "Returned";
    private const string StatusOverdue = "Overdue";
    private const string RoleStudent = "Student";

    public async Task<BookIssueListResponseDto> GetListAsync(BookIssueFilterDto filter, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        await RefreshOverdueAsync(ct);

        var page = filter.Page < 1 ? 1 : filter.Page;
        var size = filter.PageSize is < 1 or > 200 ? 25 : filter.PageSize;
        var (items, total) = await uow.BookIssues.SearchAsync(new BookIssueSearchFilter
        {
            Status = filter.Status,
            Role = filter.Role,
            Search = filter.Search,
            Page = page,
            PageSize = size
        }, ct);

        return new BookIssueListResponseDto
        {
            Data = items.Select((x, i) => MapList(x, (page - 1) * size + i + 1)).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = size,
            TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)size)
        };
    }

    public async Task<IReadOnlyList<BookIssueListItemDto>> GetMyAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        await RefreshOverdueAsync(ct);

        var userId = CurrentUser();
        var employee = await uow.Employees.GetByUserIdAsync(userId, ct);
        if (employee is not null)
        {
            var issues = await uow.BookIssues.GetMyAsync(null, employee.Id, ct);
            return issues.Select((x, i) => MapList(x, i + 1)).ToList();
        }

        var student = await uow.Students.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException("No employee or student profile found for current user.");
        var studentIssues = await uow.BookIssues.GetMyAsync(student.Id, null, ct);
        return studentIssues.Select((x, i) => MapList(x, i + 1)).ToList();
    }

    public async Task<BookIssueListItemDto> IssueAsync(IssueBookDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();

        var book = await uow.Books.GetByIdAsync(dto.BookId, ct) ?? throw new NotFoundException($"Book '{dto.BookId}' not found.");
        var available = book.TotalStock - book.IssuedCopies;
        if (available <= 0)
            throw new AppException("No available copies of this book to issue.", 400);

        if (dto.DateOfExpiry.Date < (dto.DateOfIssue ?? DateTime.UtcNow).Date)
            throw new AppException("Expiry date cannot be before the issue date.", 400);

        string userName;
        Guid? studentId = null;
        Guid? employeeId = null;
        var role = dto.Role.Trim();

        if (role.Equals(RoleStudent, StringComparison.OrdinalIgnoreCase))
        {
            if (!dto.StudentId.HasValue)
                throw new AppException("StudentId is required for Student role.", 400);
            var student = await uow.Students.GetByIdAsync(dto.StudentId.Value, ct)
                ?? throw new NotFoundException("Student not found.");
            studentId = student.Id;
            userName = string.IsNullOrWhiteSpace(student.LastName) ? student.FirstName.Trim() : $"{student.FirstName.Trim()} {student.LastName.Trim()}";
        }
        else
        {
            if (!dto.EmployeeId.HasValue)
                throw new AppException("EmployeeId is required for staff roles.", 400);
            var employee = await uow.Employees.GetByIdAsync(dto.EmployeeId.Value, ct)
                ?? throw new NotFoundException("Employee not found.");
            employeeId = employee.Id;
            userName = employee.Name;
        }

        var entity = new BookIssue
        {
            Id = Guid.NewGuid(),
            BookId = book.Id,
            Role = role,
            StudentId = studentId,
            EmployeeId = employeeId,
            UserName = userName,
            DateOfIssue = (dto.DateOfIssue ?? DateTime.UtcNow).Date,
            DateOfExpiry = dto.DateOfExpiry.Date,
            Fine = 0,
            Status = StatusIssued,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        book.IssuedCopies += 1;
        book.UpdatedAt = DateTime.UtcNow;

        await uow.BookIssues.AddAsync(entity, ct);
        await uow.Books.UpdateAsync(book, ct);
        await uow.SaveTenantChangesAsync(ct);
        return MapList(await uow.BookIssues.GetByIdAsync(entity.Id, ct) ?? entity, 0);
    }

    public async Task<BookIssueListItemDto> ReturnAsync(Guid id, ReturnBookDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();

        var entity = await uow.BookIssues.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Book issue '{id}' not found.");
        if (entity.Status == StatusReturned)
            throw new AppException("This book has already been returned.", 400);

        var returnDate = (dto.ReturnDate ?? DateTime.UtcNow).Date;
        var overdueDays = (returnDate - entity.DateOfExpiry.Date).Days;
        var fine = dto.FineOverride ?? (overdueDays > 0 ? overdueDays * AppConstants.LibraryFinePerDay : 0);

        entity.ReturnDate = returnDate;
        entity.Fine = fine;
        entity.Status = StatusReturned;
        entity.UpdatedAt = DateTime.UtcNow;

        var book = await uow.Books.GetByIdAsync(entity.BookId, ct);
        if (book is not null)
        {
            book.IssuedCopies = Math.Max(0, book.IssuedCopies - 1);
            book.UpdatedAt = DateTime.UtcNow;
            await uow.Books.UpdateAsync(book, ct);
        }

        await uow.BookIssues.UpdateAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
        return MapList(await uow.BookIssues.GetByIdAsync(id, ct) ?? entity, 0);
    }

    public async Task<IReadOnlyList<BorrowerLookupDto>> GetBorrowersLookupAsync(string role, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();

        if (string.IsNullOrWhiteSpace(role))
            throw new AppException("Role is required.", 400);

        if (role.Trim().Equals(RoleStudent, StringComparison.OrdinalIgnoreCase))
        {
            var (students, _) = await uow.Students.SearchAsync(new StudentSearchFilter
            {
                IsActive = true,
                Page = 1,
                PageSize = 500
            }, ct);
            return students.Select(s => new BorrowerLookupDto
            {
                Id = s.Id,
                Name = string.IsNullOrWhiteSpace(s.LastName) ? s.FirstName.Trim() : $"{s.FirstName.Trim()} {s.LastName.Trim()}",
                Code = s.RegisterNo
            }).ToList();
        }

        var (employees, _) = await uow.Employees.SearchAsync(new EmployeeSearchFilter
        {
            Role = role.Trim(),
            IsActive = true,
            Page = 1,
            PageSize = 500
        }, ct);
        return employees.Select(e => new BorrowerLookupDto
        {
            Id = e.Id,
            Name = e.Name,
            Code = e.StaffId
        }).ToList();
    }

    private async Task RefreshOverdueAsync(CancellationToken ct)
    {
        var issued = await uow.BookIssues.GetIssuedAsync(ct);
        var today = DateTime.UtcNow.Date;
        var changed = false;
        foreach (var issue in issued)
        {
            if (issue.Status == StatusIssued && issue.DateOfExpiry.Date < today)
            {
                issue.Status = StatusOverdue;
                issue.UpdatedAt = DateTime.UtcNow;
                await uow.BookIssues.UpdateAsync(issue, ct);
                changed = true;
            }
        }

        if (changed)
            await uow.SaveTenantChangesAsync(ct);
    }

    private BookIssueListItemDto MapList(BookIssue i, int sl) => new()
    {
        Id = i.Id,
        Sl = sl,
        Branch = tenant.TenantName ?? string.Empty,
        BookId = i.BookId,
        BookTitle = i.Book.Title,
        Role = i.Role,
        BorrowerName = i.UserName ?? string.Empty,
        DateOfIssue = i.DateOfIssue,
        DateOfExpiry = i.DateOfExpiry,
        ReturnDate = i.ReturnDate,
        Fine = i.Fine,
        Status = i.Status
    };

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
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin) &&
            !r.Contains(AppConstants.Roles.Librarian))
            throw new ForbiddenException("Only Super Admin, School Admin, or Librarian can manage book issues.");
    }

    private Guid CurrentUser()
    {
        var c = http.HttpContext?.User.FindFirst(AppConstants.Claims.UserId)
            ?? http.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        if (c is null || !Guid.TryParse(c.Value, out var id)) throw new UnauthorizedException();
        return id;
    }
}
