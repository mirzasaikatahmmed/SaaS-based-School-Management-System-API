using SchoolManagement.BLL.DTOs.Parents;

namespace SchoolManagement.BLL.Interfaces;

public interface IParentLoginDeactivateService
{
    Task<ParentLoginDeactivateListResponseDto> GetListAsync(ParentLoginDeactivateFilterDto filter, CancellationToken cancellationToken = default);
    Task ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BulkParentLoginActivateResultDto> BulkActivateAsync(BulkParentLoginActivateDto dto, CancellationToken cancellationToken = default);
    Task<(byte[] Content, string ContentType, string FileName)> ExportAsync(ParentLoginDeactivateFilterDto filter, CancellationToken cancellationToken = default);
}
