using SchoolManagement.BLL.DTOs.Attendance;

namespace SchoolManagement.BLL.Interfaces;

public interface IEmployeeAttendanceService
{
    Task<EmployeeAttendanceForDateResponseDto> GetForDateAsync(string? role, DateTime date, CancellationToken cancellationToken = default);
    Task<EmployeeAttendanceForDateResponseDto> SaveAsync(SaveEmployeeAttendanceDto dto, CancellationToken cancellationToken = default);
    Task<EmployeeAttendanceReportResponseDto> GetReportAsync(EmployeeAttendanceReportFilterDto filter, CancellationToken cancellationToken = default);
    Task<EmployeeAttendanceReportResponseDto> GetMyReportAsync(EmployeeAttendanceReportFilterDto filter, CancellationToken cancellationToken = default);
}
