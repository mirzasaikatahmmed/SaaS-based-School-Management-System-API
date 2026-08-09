using SchoolManagement.BLL.DTOs.Payroll;

namespace SchoolManagement.BLL.Interfaces;

public interface ISalaryAssignService
{
    Task<SalaryAssignListResponseDto> GetListAsync(SalaryAssignFilterDto filter, CancellationToken cancellationToken = default);
    Task<SalaryAssignItemDto> AssignAsync(Guid employeeId, AssignSalaryGradeDto dto, CancellationToken cancellationToken = default);
    Task<BulkAssignSalaryResultDto> BulkAssignAsync(BulkAssignSalaryDto dto, CancellationToken cancellationToken = default);
}
