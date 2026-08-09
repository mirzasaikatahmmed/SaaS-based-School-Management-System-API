using SchoolManagement.BLL.DTOs.Biometric;

namespace SchoolManagement.BLL.Interfaces;

public interface IBiometricPunchService
{
    Task<PunchLogListResponseDto> GetPunchesAsync(PunchLogFilterDto filter, CancellationToken cancellationToken = default);
    Task<PunchLogItemDto> RecordManualPunchAsync(ManualPunchDto dto, CancellationToken cancellationToken = default);
}
