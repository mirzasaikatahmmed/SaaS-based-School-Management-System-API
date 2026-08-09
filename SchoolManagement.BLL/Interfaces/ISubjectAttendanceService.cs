using SchoolManagement.BLL.DTOs.Attendance;

namespace SchoolManagement.BLL.Interfaces;

public interface ISubjectAttendanceService
{
    Task<SubjectAttendanceForDateResponseDto> GetForDateAsync(
        Guid classId, Guid sectionId, Guid subjectId, DateTime date, CancellationToken cancellationToken = default);

    Task<SubjectAttendanceForDateResponseDto> SaveAsync(
        SaveSubjectAttendanceDto dto, CancellationToken cancellationToken = default);
}
