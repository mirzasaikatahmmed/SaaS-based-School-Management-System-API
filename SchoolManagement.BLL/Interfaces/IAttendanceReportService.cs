using SchoolManagement.BLL.DTOs.Reports;

namespace SchoolManagement.BLL.Interfaces;

public interface IAttendanceReportService
{
    Task<MonthlyAttendanceGridDto> GetStudentMonthlyAsync(
        Guid classId, Guid sectionId, int year, int month, CancellationToken cancellationToken = default);

    Task<StudentDailyClassReportDto> GetStudentDailyAsync(
        DateTime date, CancellationToken cancellationToken = default);

    Task<StudentOverviewReportDto> GetStudentOverviewAsync(
        Guid classId, Guid sectionId, string attendanceType, DateTime fromDate, DateTime toDate,
        CancellationToken cancellationToken = default);

    Task<SubjectWiseByDateReportDto> GetSubjectWiseAsync(
        Guid classId, Guid sectionId, Guid subjectId, DateTime date, CancellationToken cancellationToken = default);

    Task<SubjectWiseDayReportDto> GetSubjectWiseByDayAsync(
        Guid classId, Guid sectionId, DateTime date, CancellationToken cancellationToken = default);

    Task<MonthlyAttendanceGridDto> GetSubjectWiseByMonthAsync(
        Guid classId, Guid sectionId, Guid subjectId, int year, int month, CancellationToken cancellationToken = default);

    Task<MonthlyAttendanceGridDto> GetEmployeeMonthlyAsync(
        string? role, int year, int month, CancellationToken cancellationToken = default);

    Task<ExamAttendanceReportDto> GetExamReportAsync(
        Guid examId, Guid classId, Guid sectionId, Guid subjectId, CancellationToken cancellationToken = default);

    Task<FingerprintLogReportDto> GetFingerprintLogsAsync(
        FingerprintLogFilterDto filter, CancellationToken cancellationToken = default);
}
