using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class StudentPromotionRepository(TenantDbContext context) : IStudentPromotionRepository
{
    public async Task<StudentPromotion> AddAsync(StudentPromotion entity, CancellationToken cancellationToken = default)
    {
        await context.StudentPromotions.AddAsync(entity, cancellationToken);
        return entity;
    }

    public async Task<IReadOnlyList<StudentPromotion>> GetByStudentAsync(Guid studentId, CancellationToken cancellationToken = default)
        => await context.StudentPromotions
            .Include(p => p.FromClass)
            .Include(p => p.FromSection)
            .Include(p => p.ToClass)
            .Include(p => p.ToSection)
            .Where(p => p.StudentId == studentId)
            .OrderByDescending(p => p.PromotedAt)
            .ToListAsync(cancellationToken);
}
