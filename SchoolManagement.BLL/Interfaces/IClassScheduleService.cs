using SchoolManagement.BLL.DTOs.Academic;

namespace SchoolManagement.BLL.Interfaces;

public interface IClassScheduleService
{
    Task<IReadOnlyList<ClassScheduleResponseDto>> GetByClassSectionAsync(Guid classId, Guid sectionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ClassScheduleResponseDto>> GetMyClassScheduleAsync(CancellationToken cancellationToken = default);
    Task<ClassScheduleResponseDto> UpsertAsync(UpsertClassScheduleDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TeacherScheduleDayDto>> GetTeacherScheduleAsync(Guid employeeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TeacherScheduleDayDto>> GetMyTeacherScheduleAsync(CancellationToken cancellationToken = default);
}
