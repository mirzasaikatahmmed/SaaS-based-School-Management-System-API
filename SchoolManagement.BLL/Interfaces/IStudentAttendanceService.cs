using SchoolManagement.BLL.DTOs.Attendance;

namespace SchoolManagement.BLL.Interfaces;

public interface IStudentAttendanceService
{
    Task<StudentAttendanceForDateResponseDto> GetForDateAsync(Guid classId, Guid sectionId, DateTime date, CancellationToken cancellationToken = default);
    Task<StudentAttendanceForDateResponseDto> SaveAsync(SaveStudentAttendanceDto dto, CancellationToken cancellationToken = default);
    Task<StudentAttendanceReportResponseDto> GetReportAsync(StudentAttendanceReportFilterDto filter, CancellationToken cancellationToken = default);
    Task<StudentAttendanceReportResponseDto> GetMyReportAsync(StudentAttendanceReportFilterDto filter, CancellationToken cancellationToken = default);
}
