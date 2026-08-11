using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class StudentRepository : IStudentRepository
{
    private readonly TenantDbContext _context;

    public StudentRepository(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<Student?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Students.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Student?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .Include(s => s.Class)
            .Include(s => s.Section)
            .Include(s => s.Category)
            .Include(s => s.TransportRoute)
            .Include(s => s.Hostel)
            .Include(s => s.Room)
            .Include(s => s.Guardians)
                .ThenInclude(g => g.User)
            .Include(s => s.User)
            .Include(s => s.DeactivateReasonRef)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Student?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .Include(s => s.Class)
            .Include(s => s.Section)
            .Include(s => s.Category)
            .Include(s => s.TransportRoute)
            .Include(s => s.Hostel)
            .Include(s => s.Room)
            .Include(s => s.Guardians)
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
    }

    public async Task<Student?> GetByRegisterNoAsync(string registerNo, CancellationToken cancellationToken = default)
    {
        var key = registerNo.Trim().ToLower();
        return await _context.Students
            .Include(s => s.Class)
            .Include(s => s.Section)
            .Include(s => s.Guardians)
            .FirstOrDefaultAsync(s => s.RegisterNo.ToLower() == key, cancellationToken);
    }

    public async Task<(IReadOnlyList<Student> Items, int TotalCount)> SearchAsync(
        StudentSearchFilter filter,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Students
            .Include(s => s.Class)
            .Include(s => s.Section)
            .Include(s => s.Category)
            .Include(s => s.Guardians)
                .ThenInclude(g => g.User)
            .Include(s => s.User)
            .Include(s => s.DeactivateReasonRef)
            .AsQueryable();

        if (filter.IsActive.HasValue)
            query = query.Where(s => s.IsActive == filter.IsActive.Value);

        if (filter.IsLoginActive.HasValue)
            query = query.Where(s => s.User.Active == filter.IsLoginActive.Value);

        if (filter.AcademicYear.HasValue)
            query = query.Where(s => s.AcademicYear == filter.AcademicYear.Value);

        if (filter.ClassId.HasValue)
            query = query.Where(s => s.ClassId == filter.ClassId.Value);

        if (filter.SectionId.HasValue)
            query = query.Where(s => s.SectionId == filter.SectionId.Value);

        if (filter.CategoryId.HasValue)
            query = query.Where(s => s.CategoryId == filter.CategoryId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLowerInvariant();
            query = query.Where(s =>
                (s.FirstName + " " + (s.LastName ?? "")).ToLower().Contains(term) ||
                s.FirstName.ToLower().Contains(term) ||
                (s.LastName != null && s.LastName.ToLower().Contains(term)) ||
                s.RegisterNo.ToLower().Contains(term) ||
                (s.Roll != null && s.Roll.ToLower().Contains(term)) ||
                (s.MobileNo != null && s.MobileNo.Contains(term)) ||
                (s.Email != null && s.Email.ToLower().Contains(term)) ||
                s.Guardians.Any(g => g.MobileNo.ToLower().Contains(term)));
        }

        var total = await query.CountAsync(cancellationToken);
        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize is < 1 or > 10_000 ? 20 : filter.PageSize;

        query = ApplySort(query, filter.SortBy, filter.SortDir);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<IReadOnlyList<Student>> GetByIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0)
            return Array.Empty<Student>();

        return await _context.Students
            .Include(s => s.User)
            .Where(s => idList.Contains(s.Id))
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<Student> ApplySort(IQueryable<Student> query, string? sortBy, string? sortDir)
    {
        var desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        var key = (sortBy ?? "roll").Trim().ToLowerInvariant();

        return key switch
        {
            "name" or "firstname" => desc
                ? query.OrderByDescending(s => s.FirstName).ThenByDescending(s => s.LastName)
                : query.OrderBy(s => s.FirstName).ThenBy(s => s.LastName),
            "classname" or "class" => desc
                ? query.OrderByDescending(s => s.Class!.Name)
                : query.OrderBy(s => s.Class!.Name),
            "sectionname" or "section" => desc
                ? query.OrderByDescending(s => s.Section!.Name)
                : query.OrderBy(s => s.Section!.Name),
            "registerno" => desc
                ? query.OrderByDescending(s => s.RegisterNo)
                : query.OrderBy(s => s.RegisterNo),
            "dateofbirth" or "dob" => desc
                ? query.OrderByDescending(s => s.DateOfBirth)
                : query.OrderBy(s => s.DateOfBirth),
            "gender" => desc
                ? query.OrderByDescending(s => s.Gender)
                : query.OrderBy(s => s.Gender),
            "isactive" => desc
                ? query.OrderByDescending(s => s.IsActive)
                : query.OrderBy(s => s.IsActive),
            "isloginactive" => desc
                ? query.OrderByDescending(s => s.User.Active)
                : query.OrderBy(s => s.User.Active),
            "createdat" => desc
                ? query.OrderByDescending(s => s.CreatedAt)
                : query.OrderBy(s => s.CreatedAt),
            _ => desc
                ? query.OrderByDescending(s => s.Roll)
                : query.OrderBy(s => s.Roll)
        };
    }

    public async Task<Student> AddAsync(Student student, CancellationToken cancellationToken = default)
    {
        await _context.Students.AddAsync(student, cancellationToken);
        return student;
    }

    public Task UpdateAsync(Student student, CancellationToken cancellationToken = default)
    {
        _context.Students.Update(student);
        return Task.CompletedTask;
    }

    public async Task<bool> RegisterNoExistsAsync(
        string registerNo,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Students.Where(s => s.RegisterNo == registerNo);
        if (excludeId.HasValue)
            query = query.Where(s => s.Id != excludeId.Value);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> SscRollExistsAsync(
        string sscRoll,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var roll = sscRoll.Trim();
        var query = _context.Students.Where(s => s.SscRoll != null && s.SscRoll == roll);
        if (excludeId.HasValue)
            query = query.Where(s => s.Id != excludeId.Value);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> RollExistsAsync(
        string roll,
        Guid classId,
        Guid sectionId,
        int academicYear,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Students.Where(s =>
            s.Roll == roll &&
            s.ClassId == classId &&
            s.SectionId == sectionId &&
            s.AcademicYear == academicYear &&
            s.IsActive);
        if (excludeId.HasValue)
            query = query.Where(s => s.Id != excludeId.Value);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<int> CountByAcademicYearAsync(int academicYear, CancellationToken cancellationToken = default)
    {
        return await _context.Students.CountAsync(s => s.AcademicYear == academicYear, cancellationToken);
    }

    public async Task<IReadOnlyList<Student>> GetByGuardianUserIdAsync(
        Guid guardianUserId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .Include(s => s.Class)
            .Include(s => s.Section)
            .Include(s => s.Category)
            .Include(s => s.Guardians)
            .Where(s => s.Guardians.Any(g => g.UserId == guardianUserId) && s.IsActive)
            .ToListAsync(cancellationToken);
    }
}
