using SchoolManagement.BLL.DTOs.Biometric;

namespace SchoolManagement.BLL.Interfaces;

public interface IBiometricUserMapService
{
    Task<IReadOnlyList<UserMapResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserMapResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserMapResponseDto> CreateAsync(CreateUserMapDto dto, CancellationToken cancellationToken = default);
    Task<UserMapResponseDto> UpdateAsync(Guid id, UpdateUserMapDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
