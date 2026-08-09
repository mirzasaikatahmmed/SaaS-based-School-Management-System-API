using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class ExamHallRepository(TenantDbContext context) : IExamHallRepository
{
    public async Task<IReadOnlyList<ExamHall>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.ExamHalls.OrderBy(h => h.HallNo).ToListAsync(cancellationToken);

    public async Task<ExamHall?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.ExamHalls.FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

    public async Task<bool> HallNoExistsAsync(string hallNo, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var n = hallNo.Trim().ToUpperInvariant();
        var q = context.ExamHalls.Where(h => h.HallNo.ToUpper() == n);
        if (excludeId.HasValue) q = q.Where(h => h.Id != excludeId.Value);
        return await q.AnyAsync(cancellationToken);
    }

    public async Task<int> CountSchedulesUsingAsync(Guid hallId, CancellationToken cancellationToken = default)
        => await context.ExamScheduleSubjects.CountAsync(s => s.HallId == hallId, cancellationToken);

    public async Task<ExamHall> AddAsync(ExamHall entity, CancellationToken cancellationToken = default)
    {
        await context.ExamHalls.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(ExamHall entity, CancellationToken cancellationToken = default)
    {
        context.ExamHalls.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ExamHall entity, CancellationToken cancellationToken = default)
    {
        context.ExamHalls.Remove(entity);
        return Task.CompletedTask;
    }
}
