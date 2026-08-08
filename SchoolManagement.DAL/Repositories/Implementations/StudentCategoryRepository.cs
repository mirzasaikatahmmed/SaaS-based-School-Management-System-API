using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class StudentCategoryRepository : IStudentCategoryRepository
{
    private readonly TenantDbContext _context;

    public StudentCategoryRepository(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<StudentCategory>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.StudentCategories
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<StudentCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.StudentCategories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> NameExistsAsync(
        string name,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim().ToUpperInvariant();
        var query = _context.StudentCategories.Where(c => c.Name.ToUpper() == normalized);
        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<int> CountStudentsUsingAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.Students.CountAsync(s => s.CategoryId == categoryId, cancellationToken);
    }

    public async Task<StudentCategory> AddAsync(StudentCategory category, CancellationToken cancellationToken = default)
    {
        await _context.StudentCategories.AddAsync(category, cancellationToken);
        return category;
    }

    public Task UpdateAsync(StudentCategory category, CancellationToken cancellationToken = default)
    {
        _context.StudentCategories.Update(category);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(StudentCategory category, CancellationToken cancellationToken = default)
    {
        _context.StudentCategories.Remove(category);
        return Task.CompletedTask;
    }
}
