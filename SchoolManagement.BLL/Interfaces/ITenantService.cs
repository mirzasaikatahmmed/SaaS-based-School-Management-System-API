using SchoolManagement.BLL.DTOs.Tenant;

namespace SchoolManagement.BLL.Interfaces;

public interface ITenantService
{
    Task<TenantResponseDto> CreateTenantAsync(CreateTenantDto request, CancellationToken cancellationToken = default);
    Task<TenantResponseDto> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<TenantResponseDto> UpdateSettingsAsync(string slug, UpdateTenantSettingsDto request, CancellationToken cancellationToken = default);
    Task DeactivateAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TenantResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
