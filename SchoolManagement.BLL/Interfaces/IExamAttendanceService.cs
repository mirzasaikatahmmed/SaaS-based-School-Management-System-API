using SchoolManagement.BLL.DTOs.Attendance;

namespace SchoolManagement.BLL.Interfaces;

public interface IExamAttendanceService
{
    Task<ExamAttendanceResponseDto> GetAsync(ExamAttendanceFilterDto filter, CancellationToken cancellationToken = default);
    Task<ExamAttendanceResponseDto> SaveAsync(SaveExamAttendanceDto dto, CancellationToken cancellationToken = default);
}
