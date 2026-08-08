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
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Student?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .Include(s => s.Class)
            .Include(s => s.Section)
            .Include(s => s.Category)
            .Include(s => s.Guardians)
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
    }

    public async Task<Student?> GetByRegisterNoAsync(string registerNo, CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .FirstOrDefaultAsync(s => s.RegisterNo == registerNo, cancellationToken);
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
            .AsQueryable();

        if (filter.IsActive.HasValue)
            query = query.Where(s => s.IsActive == filter.IsActive.Value);

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
                s.FirstName.ToLower().Contains(term) ||
                (s.LastName != null && s.LastName.ToLower().Contains(term)) ||
                s.RegisterNo.ToLower().Contains(term) ||
                (s.Roll != null && s.Roll.ToLower().Contains(term)) ||
                (s.MobileNo != null && s.MobileNo.Contains(term)) ||
                (s.Email != null && s.Email.ToLower().Contains(term)));
        }

        var total = await query.CountAsync(cancellationToken);
        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize is < 1 or > 200 ? 20 : filter.PageSize;

        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
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
