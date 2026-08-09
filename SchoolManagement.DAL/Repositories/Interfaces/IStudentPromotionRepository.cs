using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IStudentPromotionRepository
{
    Task<StudentPromotion> AddAsync(StudentPromotion entity, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StudentPromotion>> GetByStudentAsync(Guid studentId, CancellationToken cancellationToken = default);
}
