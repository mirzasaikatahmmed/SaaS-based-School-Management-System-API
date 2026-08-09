using SchoolManagement.BLL.DTOs.Biometric;

namespace SchoolManagement.BLL.Interfaces;

public interface IBiometricDeviceService
{
    Task<IReadOnlyList<DeviceResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DeviceResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DeviceResponseDto> CreateAsync(CreateDeviceDto dto, CancellationToken cancellationToken = default);
    Task<DeviceResponseDto> UpdateAsync(Guid id, UpdateDeviceDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
