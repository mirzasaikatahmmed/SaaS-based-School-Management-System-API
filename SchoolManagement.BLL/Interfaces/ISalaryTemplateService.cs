using SchoolManagement.BLL.DTOs.Payroll;

namespace SchoolManagement.BLL.Interfaces;

public interface ISalaryTemplateService
{
    Task<IReadOnlyList<SalaryTemplateListItemDto>> GetListAsync(CancellationToken cancellationToken = default);
    Task<SalaryTemplateResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SalaryTemplateLookupDto>> GetLookupAsync(CancellationToken cancellationToken = default);
    Task<SalaryTemplateResponseDto> CreateAsync(CreateSalaryTemplateDto dto, CancellationToken cancellationToken = default);
    Task<SalaryTemplateResponseDto> UpdateAsync(Guid id, UpdateSalaryTemplateDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
