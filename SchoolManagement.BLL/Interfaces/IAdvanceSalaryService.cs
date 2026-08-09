using SchoolManagement.BLL.DTOs.AdvanceSalary;

namespace SchoolManagement.BLL.Interfaces;

public interface IAdvanceSalaryService
{
    Task<AdvanceSalaryMyListResponseDto> GetMyListAsync(AdvanceSalaryFilterDto filter, CancellationToken cancellationToken = default);
    Task<AdvanceSalaryResponseDto> CreateMyAsync(CreateMyAdvanceSalaryDto dto, CancellationToken cancellationToken = default);
    Task<AdvanceSalaryListResponseDto> GetManageListAsync(AdvanceSalaryManageFilterDto filter, CancellationToken cancellationToken = default);
    Task<AdvanceSalaryResponseDto> CreateForEmployeeAsync(CreateAdvanceSalaryDto dto, CancellationToken cancellationToken = default);
    Task<AdvanceSalaryResponseDto> ApproveAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AdvanceSalaryResponseDto> RejectAsync(Guid id, ReviewAdvanceSalaryDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(byte[] Content, string ContentType, string FileName)> ExportAsync(AdvanceSalaryManageFilterDto filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<HrEmployeeLookupDto>> GetEmployeeLookupAsync(string role, CancellationToken cancellationToken = default);
}
