using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IExamHallRepository
{
    Task<IReadOnlyList<ExamHall>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ExamHall?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> HallNoExistsAsync(string hallNo, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<int> CountSchedulesUsingAsync(Guid hallId, CancellationToken cancellationToken = default);
    Task<ExamHall> AddAsync(ExamHall entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(ExamHall entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(ExamHall entity, CancellationToken cancellationToken = default);
}
