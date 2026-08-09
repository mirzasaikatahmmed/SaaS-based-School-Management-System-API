using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IClassScheduleRepository
{
    Task<IReadOnlyList<ClassSchedule>> GetByClassSectionAsync(Guid classId, Guid sectionId, CancellationToken cancellationToken = default);
    Task<ClassSchedule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ClassSchedule?> GetByClassSectionDayAsync(Guid classId, Guid sectionId, string day, CancellationToken cancellationToken = default);
    Task<ClassSchedule> AddAsync(ClassSchedule entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(ClassSchedule entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(ClassSchedule entity, CancellationToken cancellationToken = default);
    Task ReplacePeriodsAsync(Guid scheduleId, IEnumerable<ClassSchedulePeriod> periods, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ClassSchedulePeriod>> GetByTeacherAsync(Guid employeeId, CancellationToken cancellationToken = default);
}
