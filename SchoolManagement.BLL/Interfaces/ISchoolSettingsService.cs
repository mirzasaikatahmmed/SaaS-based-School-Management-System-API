using SchoolManagement.BLL.DTOs.Settings;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.BLL.Interfaces;

public interface ISchoolSettingsService
{
    Task<SchoolListResponseDto> GetSchoolListAsync(SchoolSearchFilter filter, CancellationToken cancellationToken = default);
    Task<SchoolSettingsResponseDto> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<SchoolSettingsResponseDto> UpdateGeneralAsync(string slug, UpdateSchoolGeneralDto dto, CancellationToken cancellationToken = default);
    Task<SchoolSettingsResponseDto> UpdateStudentPanelAsync(string slug, UpdateStudentPanelDto dto, CancellationToken cancellationToken = default);
    Task<SchoolSettingsResponseDto> UpdatePaymentAsync(string slug, PaymentSettingsDto dto, CancellationToken cancellationToken = default);
    Task<SchoolSettingsResponseDto> UploadLogoAsync(string slug, string type, Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default);
}
