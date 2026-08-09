using SchoolManagement.BLL.DTOs.Settings;

namespace SchoolManagement.BLL.Interfaces;

public interface IGlobalSettingsService
{
    Task<GlobalSettingsResponseDto> GetAsync(CancellationToken cancellationToken = default);
    Task<GlobalSettingsResponseDto> UpdateGeneralAsync(UpdateGlobalGeneralDto dto, CancellationToken cancellationToken = default);
    Task<GlobalSettingsResponseDto> UpdateUploadFileAsync(UpdateGlobalUploadFileDto dto, CancellationToken cancellationToken = default);
}
