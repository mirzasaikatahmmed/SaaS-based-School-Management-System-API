using SchoolManagement.BLL.DTOs.School;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.BLL.Interfaces;

public interface ISchoolService
{
    Task<SchoolListResponseDto> GetSchoolsAsync(SchoolSearchFilter filter, CancellationToken cancellationToken = default);
    Task<SchoolResponseDto> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<SchoolResponseDto> CreateSchoolAsync(CreateSchoolDto dto, CancellationToken cancellationToken = default);
    Task<SchoolResponseDto> UpdateSchoolAsync(string slug, UpdateSchoolDto dto, CancellationToken cancellationToken = default);
    Task DeactivateAsync(string slug, CancellationToken cancellationToken = default);
    Task ActivateAsync(string slug, CancellationToken cancellationToken = default);
    Task<SchoolResponseDto> UploadLogoAsync(string slug, Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task<SchoolSettingsDto> GetSettingsAsync(string slug, CancellationToken cancellationToken = default);
    Task<SchoolSettingsDto> UpdateSettingsAsync(string slug, SchoolSettingsDto dto, CancellationToken cancellationToken = default);
    Task<SchoolStatsDto> GetStatsAsync(string slug, CancellationToken cancellationToken = default);
    Task<(byte[] Content, string ContentType, string FileName)> ExportAsync(SchoolSearchFilter filter, string format, CancellationToken cancellationToken = default);
}
