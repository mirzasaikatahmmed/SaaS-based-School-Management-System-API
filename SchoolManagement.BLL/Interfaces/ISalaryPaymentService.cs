using SchoolManagement.BLL.DTOs.Payroll;

namespace SchoolManagement.BLL.Interfaces;

public interface ISalaryPaymentService
{
    Task<SalaryPaymentListResponseDto> GetListAsync(SalaryPaymentFilterDto filter, CancellationToken cancellationToken = default);
    Task<SalaryPaymentResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SalaryPaymentResponseDto> ProcessPaymentAsync(Guid employeeId, ProcessPaymentDto dto, CancellationToken cancellationToken = default);
    Task<SalaryPaymentResponseDto> UpdatePaymentAsync(Guid id, ProcessPaymentDto dto, CancellationToken cancellationToken = default);
    Task<(byte[] Content, string ContentType, string FileName)> ExportAsync(SalaryPaymentFilterDto filter, CancellationToken cancellationToken = default);
    Task<MySalaryDto> GetMySalaryAsync(CancellationToken cancellationToken = default);
    Task<SalaryPaymentResponseDto> GetMySalaryForMonthAsync(string month, CancellationToken cancellationToken = default);
}
