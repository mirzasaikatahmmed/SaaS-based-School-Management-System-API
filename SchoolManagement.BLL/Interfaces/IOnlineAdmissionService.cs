using SchoolManagement.BLL.DTOs.OnlineAdmission;
using SchoolManagement.BLL.DTOs.Student;

namespace SchoolManagement.BLL.Interfaces;

public interface IOnlineAdmissionService
{
    Task<OnlineAdmissionResponseDto> ApplyAsync(SubmitOnlineAdmissionDto dto, CancellationToken cancellationToken = default);
    Task<OnlineAdmissionTrackDto> TrackAsync(string referenceNo, string? tenantSlug = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdmissionLookupItemDto>> GetPublicClassesAsync(string tenantSlug, CancellationToken cancellationToken = default);

    Task<OnlineAdmissionListResponseDto> GetListAsync(OnlineAdmissionFilterDto filter, CancellationToken cancellationToken = default);
    Task<OnlineAdmissionResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OnlineAdmissionResponseDto> ApproveAsync(Guid id, ApproveAdmissionDto dto, CancellationToken cancellationToken = default);
    Task<OnlineAdmissionResponseDto> DeclineAsync(Guid id, DeclineAdmissionDto dto, CancellationToken cancellationToken = default);
    Task<OnlineAdmissionResponseDto> UpdatePaymentAsync(Guid id, UpdatePaymentStatusDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OnlineAdmissionResponseDto> GetPrintDataAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(byte[] Content, string ContentType, string FileName)> ExportAsync(OnlineAdmissionFilterDto filter, CancellationToken cancellationToken = default);
}
