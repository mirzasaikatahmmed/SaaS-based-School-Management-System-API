using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class GuardianRepository : IGuardianRepository
{
    private readonly TenantDbContext _context;

    public GuardianRepository(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<Guardian?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Guardians.FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<Guardian?> GetByIdWithStudentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Guardians
            .Include(g => g.Student)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Guardian>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        return await _context.Guardians
            .Where(g => g.StudentId == studentId)
            .OrderByDescending(g => g.IsPrimary)
            .ThenBy(g => g.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Guardian> AddAsync(Guardian guardian, CancellationToken cancellationToken = default)
    {
        await _context.Guardians.AddAsync(guardian, cancellationToken);
        return guardian;
    }

    public Task UpdateAsync(Guardian guardian, CancellationToken cancellationToken = default)
    {
        _context.Guardians.Update(guardian);
        return Task.CompletedTask;
    }
}
