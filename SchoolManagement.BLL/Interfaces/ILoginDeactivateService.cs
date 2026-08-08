using SchoolManagement.BLL.DTOs.StudentDetails;

namespace SchoolManagement.BLL.Interfaces;

public interface ILoginDeactivateService
{
    Task<LoginDeactivateListResponseDto> GetListAsync(LoginDeactivateFilterDto filter, CancellationToken cancellationToken = default);
    Task ActivateAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task DeactivateAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task<BulkAuthenticationActivateResultDto> BulkActivateAsync(BulkAuthenticationActivateDto dto, CancellationToken cancellationToken = default);
    Task<(byte[] Content, string ContentType, string FileName)> ExportAsync(LoginDeactivateFilterDto filter, CancellationToken cancellationToken = default);
}
