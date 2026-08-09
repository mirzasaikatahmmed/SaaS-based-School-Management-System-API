using SchoolManagement.BLL.DTOs.StudentAccounting;

namespace SchoolManagement.BLL.Interfaces;

public interface IStudentFeeInvoiceService
{
    Task<StudentFeeInvoiceListResponseDto> GetFilteredAsync(StudentFeeInvoiceFilterDto filter, CancellationToken cancellationToken = default);
    Task<StudentFeeInvoiceListResponseDto> GetDueAsync(DueInvoiceFilterDto filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StudentFeeInvoiceResponseDto>> GetMyInvoicesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StudentFeeInvoiceResponseDto>> GetByStudentAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task<StudentFeeInvoiceResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<StudentFeeInvoiceResponseDto> PayAsync(Guid id, PayInvoiceDto dto, CancellationToken cancellationToken = default);
    Task<GenerateInvoicesResultDto> GenerateAsync(GenerateInvoicesDto dto, CancellationToken cancellationToken = default);
    Task<(byte[] Content, string ContentType, string FileName)> ExportAsync(StudentFeeInvoiceFilterDto filter, CancellationToken cancellationToken = default);
}
