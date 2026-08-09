using SchoolManagement.BLL.DTOs.Reports;

namespace SchoolManagement.BLL.Interfaces;

public interface IHrReportService
{
    Task<LeaveReportDto> GetLeaveReportAsync(LeaveReportFilterDto filter, CancellationToken cancellationToken = default);
    Task<PayrollSummaryReportDto> GetPayrollSummaryAsync(PayrollSummaryFilterDto filter, CancellationToken cancellationToken = default);
}
